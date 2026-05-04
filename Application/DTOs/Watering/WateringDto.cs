namespace FloraLink.Application.DTOs.Watering;

public class WateringDto
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public DateTime WateredAt { get; set; }
    public double WaterAmountMl { get; set; }
    public bool IsAutomatic { get; set; }
    public string? Notes { get; set; }
}
