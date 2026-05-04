namespace FloraLink.Domain.Entities;

public class WateringEvent
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public Plant Plant { get; set; } = null!;

    public DateTime WateredAt { get; set; } = DateTime.UtcNow;
    public double WaterAmountMl { get; set; }
    public bool IsAutomatic { get; set; }
    public string? Notes { get; set; }
}
