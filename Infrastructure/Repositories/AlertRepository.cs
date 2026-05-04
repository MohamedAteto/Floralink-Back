using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;
using FloraLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FloraLink.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly FloraLinkDbContext _db;

    public AlertRepository(FloraLinkDbContext db) => _db = db;

    public async Task<IEnumerable<Alert>> GetByPlantIdAsync(int plantId) =>
        await _db.Alerts
            .Where(a => a.PlantId == plantId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Alert>> GetUnreadByUserIdAsync(int userId) =>
        await _db.Alerts
            .Include(a => a.Plant)
            .Where(a => a.Plant.UserId == userId && !a.IsRead)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<Alert> AddAsync(Alert alert)
    {
        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync();
        return alert;
    }

    public async Task MarkAsReadAsync(int alertId)
    {
        var alert = await _db.Alerts.FindAsync(alertId);
        if (alert != null)
        {
            alert.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }
}
