namespace FloraLink.Application.DTOs.Plants;

public class PlantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SensorId { get; set; } = string.Empty;
    public string PlantTypeName { get; set; } = string.Empty;
    public int PlantTypeId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Latest reading summary
    public double? LatestMoisture { get; set; }
    public double? LatestTemperature { get; set; }
    public double? HealthScore { get; set; }
    public string? Status { get; set; }
    public DateTime? LastReadingAt { get; set; }
    public DateTime? LastWateredAt { get; set; }
    public string? PredictedNextWatering { get; set; }

    // Plant type ideal ranges (for display on PlantDetails)
    public double? PlantTypeMinMoisture { get; set; }
    public double? PlantTypeMaxMoisture { get; set; }
    public double? PlantTypeMinTemperature { get; set; }
    public double? PlantTypeMaxTemperature { get; set; }
    public double? PlantTypeCriticalMoisture { get; set; }
}
