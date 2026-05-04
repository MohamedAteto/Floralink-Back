using FloraLink.Application.DTOs.Plants;
using FloraLink.Application.Interfaces;
using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;

namespace FloraLink.Application.Services;

public class PlantService : IPlantService
{
    private readonly IPlantRepository _plants;
    private readonly ISensorReadingRepository _readings;
    private readonly IWateringRepository _watering;
    private readonly IPlantTypeRepository _plantTypes;

    public PlantService(
        IPlantRepository plants,
        ISensorReadingRepository readings,
        IWateringRepository watering,
        IPlantTypeRepository plantTypes)
    {
        _plants = plants;
        _readings = readings;
        _watering = watering;
        _plantTypes = plantTypes;
    }

    public async Task<IEnumerable<PlantDto>> GetUserPlantsAsync(int userId)
    {
        var plants = await _plants.GetAllByUserIdAsync(userId);
        var result = new List<PlantDto>();

        foreach (var plant in plants)
            result.Add(await MapToDtoAsync(plant));

        return result;
    }

    public async Task<PlantDto?> GetPlantByIdAsync(int plantId, int userId)
    {
        var plant = await _plants.GetByIdAsync(plantId);
        if (plant == null || plant.UserId != userId) return null;
        return await MapToDtoAsync(plant);
    }

    public async Task<PlantDto> CreatePlantAsync(CreatePlantDto dto, int userId)
    {
        var plantType = await _plantTypes.GetByIdAsync(dto.PlantTypeId)
            ?? throw new ArgumentException("Invalid plant type.");

        var plant = new Plant
        {
            Name = dto.Name,
            SensorId = dto.SensorId,
            PlantTypeId = dto.PlantTypeId,
            UserId = userId
        };

        var created = await _plants.AddAsync(plant);
        created.PlantType = plantType;
        return await MapToDtoAsync(created);
    }

    public async Task DeletePlantAsync(int plantId, int userId)
    {
        var plant = await _plants.GetByIdAsync(plantId);
        if (plant == null || plant.UserId != userId)
            throw new UnauthorizedAccessException("Plant not found.");
        await _plants.DeleteAsync(plantId);
    }

    private async Task<PlantDto> MapToDtoAsync(Plant plant)
    {
        var latest = await _readings.GetLatestByPlantIdAsync(plant.Id);
        var lastWatering = await _watering.GetLastWateringAsync(plant.Id);
        string? predictedWatering = null;

        if (latest != null)
        {
            var recentReadings = (await _readings.GetRecentAsync(plant.Id, 3)).ToList();
            predictedWatering = PredictNextWatering(recentReadings, plant.PlantType?.CriticalMoistureThreshold ?? 20);
        }

        return new PlantDto
        {
            Id = plant.Id,
            Name = plant.Name,
            SensorId = plant.SensorId,
            PlantTypeId = plant.PlantTypeId,
            PlantTypeName = plant.PlantType?.Name ?? string.Empty,
            CreatedAt = plant.CreatedAt,
            LatestMoisture = latest?.SoilMoisture,
            LatestTemperature = latest?.Temperature,
            HealthScore = latest?.HealthScore,
            Status = latest?.PlantStatus,
            LastReadingAt = latest?.RecordedAt,
            LastWateredAt = lastWatering?.WateredAt,
            PredictedNextWatering = predictedWatering,
            PlantTypeMinMoisture = plant.PlantType?.MinMoisture,
            PlantTypeMaxMoisture = plant.PlantType?.MaxMoisture,
            PlantTypeMinTemperature = plant.PlantType?.MinTemperature,
            PlantTypeMaxTemperature = plant.PlantType?.MaxTemperature,
            PlantTypeCriticalMoisture = plant.PlantType?.CriticalMoistureThreshold
        };
    }

    private static string? PredictNextWatering(List<SensorReading> readings, double criticalThreshold)
    {
        if (readings.Count < 2) return null;

        // Calculate average daily moisture drop rate
        var ordered = readings.OrderByDescending(r => r.RecordedAt).ToList();
        double totalDrop = 0;
        int count = 0;

        for (int i = 0; i < ordered.Count - 1; i++)
        {
            double drop = ordered[i + 1].SoilMoisture - ordered[i].SoilMoisture;
            double hours = (ordered[i].RecordedAt - ordered[i + 1].RecordedAt).TotalHours;
            if (hours > 0 && drop < 0)
            {
                totalDrop += Math.Abs(drop) / hours * 24; // per day
                count++;
            }
        }

        if (count == 0) return null;

        double dropRatePerDay = totalDrop / count;
        double currentMoisture = ordered[0].SoilMoisture;
        double daysUntilCritical = (currentMoisture - criticalThreshold) / dropRatePerDay;

        if (daysUntilCritical <= 0) return "Now";

        var predictedDate = DateTime.UtcNow.AddDays(daysUntilCritical);
        return predictedDate.ToString("MMM dd, yyyy HH:mm");
    }
}
