using FloraLink.Application.DTOs.Alerts;

namespace FloraLink.Application.Interfaces;

public interface IAlertService
{
    Task<IEnumerable<AlertDto>> GetUserAlertsAsync(int userId);
    Task MarkAlertReadAsync(int alertId);
}
