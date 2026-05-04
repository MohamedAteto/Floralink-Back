namespace FloraLink.Domain.Entities;

public class PlantDiaryEntry
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public Plant Plant { get; set; } = null!;

    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
}
