using FloraLink.Application.Interfaces;
using FloraLink.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FloraLink.API.Controllers;

[ApiController]
[Route("api/plant-types")]
public class PlantTypesController : ControllerBase
{
    private readonly IPlantTypeService _service;

    public PlantTypesController(IPlantTypeService service) => _service = service;

    // Returns all seeded + AI-saved plant types
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var types = await _service.GetAllAsync();
        return Ok(types.Select(t => new
        {
            t.Id,
            t.Name,
            t.Emoji,
            t.Category,
            t.Description,
            t.MinMoisture,
            t.MaxMoisture,
            t.MinTemperature,
            t.MaxTemperature,
            t.CriticalMoistureThreshold,
            t.WateringFrequency,
            t.IsAIGenerated
        }));
    }

    // DB-first lookup, AI fallback if not found — saves result for future use
    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Plant name is required.");

        var result = await _service.GetOrCreateByNameAsync(name.Trim());
        if (result == null)
            return Ok(new { found = false, message = $"No data found for '{name}'. AI service may be unavailable." });

        return Ok(new
        {
            found = true,
            result.Id, result.Name, result.Emoji, result.Category, result.Description,
            result.MinMoisture, result.MaxMoisture, result.MinTemperature, result.MaxTemperature,
            result.CriticalMoistureThreshold, result.WateringFrequency, result.IsAIGenerated
        });
    }
}
