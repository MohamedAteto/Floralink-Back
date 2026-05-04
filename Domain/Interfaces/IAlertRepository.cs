using FloraLink.Domain.Entities;

namespace FloraLink.Domain.Interfaces;

public interface IAlertRepository
{
    Task<IEnumerable<Alert>> GetByPlantIdAsync(int plantId);
    Task<IEnumerable<Alert>> GetUnreadByUserIdAsync(int userId);
    Task<Alert> AddAsync(Alert alert);
    Task MarkAsReadAsync(int alertId);
}
