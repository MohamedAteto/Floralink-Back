namespace FloraLink.Domain.Entities;

public class Alert
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public Plant Plant { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Info, Warning, Critical
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
