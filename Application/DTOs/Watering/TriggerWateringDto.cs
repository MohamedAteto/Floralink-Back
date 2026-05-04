using System.ComponentModel.DataAnnotations;

namespace FloraLink.Application.DTOs.Watering;

public class TriggerWateringDto
{
    [Required] public int PlantId { get; set; }
    public double WaterAmountMl { get; set; } = 200;
    public string? Notes { get; set; }
}
