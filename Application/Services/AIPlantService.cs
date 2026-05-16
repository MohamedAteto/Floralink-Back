using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FloraLink.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FloraLink.Application.Services;

public class AIPlantService
{
    private readonly IConfiguration _config;
    private readonly ILogger<AIPlantService> _logger;
    private readonly HttpClient _http;

    private const int MaxAttempts = 3;
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)];

    public AIPlantService(IConfiguration config, ILogger<AIPlantService> logger, IHttpClientFactory httpFactory)
    {
        _config = config;
        _logger = logger;
        _http = httpFactory.CreateClient();
    }

    public async Task<PlantType?> FetchPlantRequirementsAsync(string plantName)
    {
        // Prefer OPENAI_API_KEY env var; fall back to appsettings OpenAI:ApiKey
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY1")
            ?? _config["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("OpenRouter API key not configured (set OPENAI_API_KEY env var or OpenAI:ApiKey in appsettings) — skipping AI fallback");
            return null;
        }

        var prompt = $"Provide plant requirements in JSON format only, no extra text:\n" +
                     $"moistureMin (0-100), moistureMax (0-100), temperatureMin (Celsius), " +
                     $"temperatureMax (Celsius), wateringFrequency\n" +
                     $"Plant name: {plantName}";

        var requestBodyJson = JsonSerializer.Serialize(new
        {
            model = "openrouter/free",
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 200,
            temperature = 0.2
        });

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var result = await TryFetchAsync(plantName, apiKey, requestBodyJson, attempt);
            if (result.Plant is not null)
                return result.Plant;

            if (!result.ShouldRetry || attempt == MaxAttempts)
                break;

            var delay = RetryDelays[attempt - 1];
            _logger.LogWarning("Retrying AI lookup for '{PlantName}' in {Delay}s (attempt {Attempt}/{Max})",
                plantName, delay.TotalSeconds, attempt, MaxAttempts);
            await Task.Delay(delay);
        }

        return null;
    }

    private async Task<(PlantType? Plant, bool ShouldRetry)> TryFetchAsync(
        string plantName, string apiKey, string requestBodyJson, int attempt)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Add("HTTP-Referer", "https://floralink.netlify.app");
            request.Headers.Add("X-Title", "FloraLink");
            request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                var shouldRetry = response.StatusCode == HttpStatusCode.TooManyRequests
                    || response.StatusCode == HttpStatusCode.ServiceUnavailable
                    || statusCode >= 500;

                _logger.LogWarning(
                    "OpenRouter returned {Status} for '{PlantName}' (attempt {Attempt}/{Max}). Retry={ShouldRetry}. Response: {Body}",
                    statusCode, plantName, attempt, MaxAttempts, shouldRetry, responseBody);

                return (null, shouldRetry);
            }

            string content;
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse OpenRouter response for '{PlantName}' (attempt {Attempt}). Raw: {Body}",
                    plantName, attempt, responseBody);
                return (null, false);
            }

            // Extract the JSON object from the AI text
            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');
            if (start < 0 || end < 0)
            {
                _logger.LogWarning("No JSON object in AI response for '{PlantName}' (attempt {Attempt}). Content: {Content}",
                    plantName, attempt, content);
                return (null, false);
            }

            var jsonPart = content[start..(end + 1)];

            using var plantDoc = JsonDocument.Parse(jsonPart);
            var root = plantDoc.RootElement;

            double moistureMin = root.TryGetProperty("moistureMin", out var mm) ? mm.GetDouble() : 40;
            double moistureMax = root.TryGetProperty("moistureMax", out var mx) ? mx.GetDouble() : 70;
            double tempMin = root.TryGetProperty("temperatureMin", out var tm) ? tm.GetDouble() : 15;
            double tempMax = root.TryGetProperty("temperatureMax", out var tx) ? tx.GetDouble() : 30;
            string watering = root.TryGetProperty("wateringFrequency", out var wf) ? wf.GetString() ?? "3-5 days" : "3-5 days";

            // Validate ranges
            moistureMin = Math.Clamp(moistureMin, 0, 100);
            moistureMax = Math.Clamp(moistureMax, moistureMin, 100);
            if (tempMin >= tempMax) tempMax = tempMin + 10;

            _logger.LogInformation("AI lookup succeeded for '{PlantName}' via OpenRouter (attempt {Attempt})", plantName, attempt);

            return (new PlantType
            {
                Name = plantName,
                Category = "Custom",
                Emoji = "🌱",
                MinMoisture = moistureMin,
                MaxMoisture = moistureMax,
                MinTemperature = tempMin,
                MaxTemperature = tempMax,
                CriticalMoistureThreshold = moistureMin * 0.6,
                WateringFrequency = watering,
                Description = $"AI-generated requirements for {plantName}",
                IsAIGenerated = true
            }, false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error calling OpenRouter for '{PlantName}' (attempt {Attempt}/{Max}) — will retry",
                plantName, attempt, MaxAttempts);
            return (null, true);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request to OpenRouter timed out for '{PlantName}' (attempt {Attempt}/{Max}) — will retry",
                plantName, attempt, MaxAttempts);
            return (null, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during AI plant fetch for '{PlantName}' (attempt {Attempt})", plantName, attempt);
            return (null, false);
        }
    }
}
