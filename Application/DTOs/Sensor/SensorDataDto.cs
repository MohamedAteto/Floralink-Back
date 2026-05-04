using System.ComponentModel.DataAnnotations;

namespace FloraLink.Application.DTOs.Sensor;

public class SensorDataDto
{
    [Required] public string SensorId { get; set; } = string.Empty;
    [Range(0, 100)] public double SoilMoisture { get; set; }
    [Range(-50, 100)] public double Temperature { get; set; }
}
