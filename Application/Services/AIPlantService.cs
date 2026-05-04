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

    public AIPlantService(IConfiguration config, ILogger<AIPlantService> logger, IHttpClientFactory httpFactory)
    {
        _config = config;
        _logger = logger;
        _http = httpFactory.CreateClient();
    }

    public async Task<PlantType?> FetchPlantRequirementsAsync(string plantName)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("OpenAI API key not configured — skipping AI fallback");
            return null;
        }

        var prompt = $"Provide plant requirements in JSON format only, no extra text:\n" +
                     $"moistureMin (0-100), moistureMax (0-100), temperatureMin (Celsius), " +
                     $"temperatureMax (Celsius), wateringFrequency\n" +
                     $"Plant name: {plantName}";

        var requestBody = new
        {
            model = "openai/gpt-3.5-turbo",
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 200,
            temperature = 0.2
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("HTTP-Referer", "https://floralink.app");
        request.Headers.Add("X-Title", "FloraLink");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI returned {Status}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            // Extract JSON from response
            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');
            if (start < 0 || end < 0) return null;
            var jsonPart = content[start..(end + 1)];

            using var plantDoc = JsonDocument.Parse(jsonPart);
            var root = plantDoc.RootElement;

            double moistureMin = root.TryGetProperty("moistureMin", out var mm) ? mm.GetDouble() : 40;
            double moistureMax = root.TryGetProperty("moistureMax", out var mx) ? mx.GetDouble() : 70;
            double tempMin = root.TryGetProperty("temperatureMin", out var tm) ? tm.GetDouble() : 15;
            double tempMax = root.TryGetProperty("temperatureMax", out var tx) ? tx.GetDouble() : 30;
            string watering = root.TryGetProperty("wateringFrequency", out var wf) ? wf.GetString() ?? "3-5 days" : "3-5 days";

            // Validate
            moistureMin = Math.Clamp(moistureMin, 0, 100);
            moistureMax = Math.Clamp(moistureMax, moistureMin, 100);
            if (tempMin >= tempMax) tempMax = tempMin + 10;

            return new PlantType
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
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI plant fetch failed for {PlantName}", plantName);
            return null;
        }
    }
}
