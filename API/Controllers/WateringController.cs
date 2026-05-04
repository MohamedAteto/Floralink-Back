using FloraLink.Application.DTOs.Watering;
using FloraLink.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloraLink.API.Controllers;

[ApiController]
[Route("api/watering")]
[Authorize]
public class WateringController : ControllerBase
{
    private readonly IWateringService _watering;

    public WateringController(IWateringService watering) => _watering = watering;

    [HttpPost]
    public async Task<IActionResult> TriggerWatering([FromBody] TriggerWateringDto dto)
    {
        var result = await _watering.TriggerWateringAsync(dto, isAutomatic: false);
        return Ok(result);
    }

    [HttpGet("{plantId}")]
    public async Task<IActionResult> GetHistory(int plantId) =>
        Ok(await _watering.GetWateringHistoryAsync(plantId));
}
