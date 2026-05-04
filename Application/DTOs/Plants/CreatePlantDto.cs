using System.ComponentModel.DataAnnotations;

namespace FloraLink.Application.DTOs.Plants;

public class CreatePlantDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string SensorId { get; set; } = string.Empty;
    [Required] public int PlantTypeId { get; set; }
}
