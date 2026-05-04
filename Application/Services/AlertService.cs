using FloraLink.Application.DTOs.Alerts;
using FloraLink.Application.Interfaces;
using FloraLink.Domain.Interfaces;

namespace FloraLink.Application.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alerts;

    public AlertService(IAlertRepository alerts)
    {
        _alerts = alerts;
    }

    public async Task<IEnumerable<AlertDto>> GetUserAlertsAsync(int userId)
    {
        var alerts = await _alerts.GetUnreadByUserIdAsync(userId);
        return alerts.Select(a => new AlertDto
        {
            Id = a.Id,
            PlantId = a.PlantId,
            PlantName = a.Plant?.Name ?? string.Empty,
            Message = a.Message,
            Severity = a.Severity,
            IsRead = a.IsRead,
            CreatedAt = a.CreatedAt
        });
    }

    public async Task MarkAlertReadAsync(int alertId)
    {
        await _alerts.MarkAsReadAsync(alertId);
    }
}
