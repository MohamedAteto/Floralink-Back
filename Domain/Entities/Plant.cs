namespace FloraLink.Domain.Entities;

public class Plant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SensorId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PlantTypeId { get; set; }
    public PlantType PlantType { get; set; } = null!;

    public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    public ICollection<WateringEvent> WateringEvents { get; set; } = new List<WateringEvent>();
    public ICollection<PlantDiaryEntry> DiaryEntries { get; set; } = new List<PlantDiaryEntry>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
