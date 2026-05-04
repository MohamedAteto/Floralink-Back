namespace FloraLink.Application.DTOs.Sensor;

public class SensorReadingDto
{
    public int Id { get; set; }
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
    public double HealthScore { get; set; }
    public string PlantStatus { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}
