namespace FloraLink.Domain.Entities;

public class SensorReading
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public Plant Plant { get; set; } = null!;

    public double SoilMoisture { get; set; }   // percentage 0–100
    public double Temperature { get; set; }    // Celsius
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Computed and stored for quick access
    public double HealthScore { get; set; }
    public string PlantStatus { get; set; } = string.Empty;
}
