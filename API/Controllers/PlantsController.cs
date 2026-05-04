using System.Security.Claims;
using FloraLink.Application.DTOs.Plants;
using FloraLink.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloraLink.API.Controllers;

[ApiController]
[Route("api/plants")]
[Authorize]
public class PlantsController : ControllerBase
{
    private readonly IPlantService _plants;

    public PlantsController(IPlantService plants) => _plants = plants;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _plants.GetUserPlantsAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plant = await _plants.GetPlantByIdAsync(id, UserId);
        return plant == null ? NotFound() : Ok(plant);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlantDto dto)
    {
        try
        {
            var plant = await _plants.CreatePlantAsync(dto, UserId);
            return CreatedAtAction(nameof(GetById), new { id = plant.Id }, plant);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _plants.DeletePlantAsync(id, UserId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound();
        }
    }
}
