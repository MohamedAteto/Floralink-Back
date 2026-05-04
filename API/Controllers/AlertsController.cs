using System.Security.Claims;
using FloraLink.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloraLink.API.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alerts;

    public AlertsController(IAlertService alerts) => _alerts = alerts;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAlerts() =>
        Ok(await _alerts.GetUserAlertsAsync(UserId));

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _alerts.MarkAlertReadAsync(id);
        return NoContent();
    }
}
