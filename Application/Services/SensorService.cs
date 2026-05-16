using FloraLink.Application.DTOs.Sensor;
using FloraLink.Application.DTOs.Watering;
using FloraLink.Application.Interfaces;
using FloraLink.Application.Services;
using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;

namespace FloraLink.Application.Services;

public class SensorService : ISensorService
{
    private readonly ISensorReadingRepository _readings;
    private readonly IPlantRepository _plants;
    private readonly IAlertRepository _alerts;
    private readonly IWateringService _wateringService;
    private readonly IWateringRepository _wateringRepository;
    private readonly EmailService _emailService;

    public SensorService(
        ISensorReadingRepository readings,
        IPlantRepository plants,
        IAlertRepository alerts,
        IWateringService wateringService,
        IWateringRepository wateringRepository,
        EmailService emailService)
    {
        _readings = readings;
        _plants = plants;
        _alerts = alerts;
        _wateringService = wateringService;
        _wateringRepository = wateringRepository;
        _emailService = emailService;
    }

    public async Task<SensorReadingDto> ProcessSensorDataAsync(SensorDataDto dto)
    {
        // Find plant by sensor ID
        var allPlants = await _plants.GetAllByUserIdAsync(0); // 0 = search all
        var plant = allPlants.FirstOrDefault(p => p.SensorId == dto.SensorId)
            ?? throw new ArgumentException($"No plant found for sensor '{dto.SensorId}'.");

        var plantType = plant.PlantType;
        double healthScore = HealthCalculator.Calculate(dto.SoilMoisture, dto.Temperature, plantType);
        string status = HealthCalculator.GetStatus(healthScore);

        var reading = new SensorReading
        {
            PlantId = plant.Id,
            SoilMoisture = dto.SoilMoisture,
            Temperature = dto.Temperature,
            HealthScore = healthScore,
            PlantStatus = status,
            RecordedAt = DateTime.UtcNow
        };

        var saved = await _readings.AddAsync(reading);

        // Generate alerts if needed
        await CheckAndCreateAlertsAsync(plant, dto, healthScore);

        // Auto-watering check
        await CheckAutoWateringAsync(plant, dto);

        return MapToDto(saved);
    }

    public async Task<SensorReadingDto?> GetLatestReadingAsync(int plantId)
    {
        var reading = await _readings.GetLatestByPlantIdAsync(plantId);
        return reading is null ? null : MapToDto(reading);
    }

    public async Task<IEnumerable<SensorReadingDto>> GetReadingsAsync(int plantId, int limit = 100)
    {
        var readings = await _readings.GetByPlantIdAsync(plantId, limit);
        return readings.Select(MapToDto);
    }

    private async Task CheckAndCreateAlertsAsync(Plant plant, SensorDataDto dto, double healthScore)
    {
        var plantType = plant.PlantType;
        var ownerEmail = plant.User?.Email ?? string.Empty;
        var ownerName = plant.User?.Username ?? "there";

        if (dto.SoilMoisture < plantType.CriticalMoistureThreshold)
        {
            var msg = $"{plant.Name} soil moisture critically low ({dto.SoilMoisture:F1}%). Water immediately!";
            await _alerts.AddAsync(new Alert
            {
                PlantId = plant.Id,
                Message = msg,
                Severity = "Critical"
            });
            if (_emailService.ShouldSendEmail(plant.Id, "CriticalMoisture"))
                await _emailService.SendPlantAlertAsync(ownerEmail, ownerName, plant.Name, msg, "Critical");
        }
        else if (dto.SoilMoisture < plantType.MinMoisture)
        {
            var msg = $"{plant.Name} needs watering soon. Moisture: {dto.SoilMoisture:F1}%";
            await _alerts.AddAsync(new Alert
            {
                PlantId = plant.Id,
                Message = msg,
                Severity = "Warning"
            });
            if (_emailService.ShouldSendEmail(plant.Id, "WarningMoisture"))
                await _emailService.SendPlantAlertAsync(ownerEmail, ownerName, plant.Name, msg, "Warning");
        }

        if (dto.Temperature > plantType.MaxTemperature + 5)
        {
            var msg = $"{plant.Name} temperature too high ({dto.Temperature:F1}°C). Move to cooler area.";
            await _alerts.AddAsync(new Alert
            {
                PlantId = plant.Id,
                Message = msg,
                Severity = "Warning"
            });
            if (_emailService.ShouldSendEmail(plant.Id, "WarningTemp"))
                await _emailService.SendPlantAlertAsync(ownerEmail, ownerName, plant.Name, msg, "Warning");
        }

        if (healthScore < 40)
        {
            var msg = $"{plant.Name} health score critical ({healthScore:F0}/100). Immediate attention needed.";
            await _alerts.AddAsync(new Alert
            {
                PlantId = plant.Id,
                Message = msg,
                Severity = "Critical"
            });
            if (_emailService.ShouldSendEmail(plant.Id, "CriticalHealth"))
                await _emailService.SendPlantAlertAsync(ownerEmail, ownerName, plant.Name, msg, "Critical");
        }
    }

    private async Task CheckAutoWateringAsync(Plant plant, SensorDataDto dto)
    {
        var plantType = plant.PlantType;
        bool moistureLow = dto.SoilMoisture < plantType.CriticalMoistureThreshold;
        bool tempHigh    = dto.Temperature > plantType.MaxTemperature + 3;

        if (!moistureLow && !tempHigh) return;

        // Cooldown: don't water again within 6 hours to prevent overwatering
        var lastWatering = await _wateringRepository.GetLastWateringAsync(plant.Id);
        if (lastWatering != null && (DateTime.UtcNow - lastWatering.WateredAt).TotalHours < 6)
            return;

        string reason = moistureLow
            ? $"moisture critically low ({dto.SoilMoisture:F1}%)"
            : $"temperature too high ({dto.Temperature:F1}°C)";

        await _wateringService.TriggerWateringAsync(new TriggerWateringDto
        {
            PlantId       = plant.Id,
            WaterAmountMl = 200,
            Notes         = $"Auto-triggered: {reason}"
        }, isAutomatic: true);
    }

    private static SensorReadingDto MapToDto(SensorReading r) => new()
    {
        Id = r.Id,
        SoilMoisture = r.SoilMoisture,
        Temperature = r.Temperature,
        HealthScore = r.HealthScore,
        PlantStatus = r.PlantStatus,
        RecordedAt = r.RecordedAt
    };
}
