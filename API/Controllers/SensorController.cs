using FloraLink.Application.DTOs.Sensor;
using FloraLink.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloraLink.API.Controllers;

[ApiController]
[Route("api/sensor-data")]
public class SensorController : ControllerBase
{
    private readonly ISensorService _sensor;

    public SensorController(ISensorService sensor) => _sensor = sensor;

    /// <summary>
    /// IoT device posts sensor readings here. No auth required for device access.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostSensorData([FromBody] SensorDataDto dto)
    {
        try
        {
            var result = await _sensor.ProcessSensorDataAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{plantId}/readings")]
    [Authorize]
    public async Task<IActionResult> GetReadings(int plantId, [FromQuery] int limit = 100) =>
        Ok(await _sensor.GetReadingsAsync(plantId, limit));
}
