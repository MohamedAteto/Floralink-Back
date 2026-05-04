using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;
using FloraLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FloraLink.Infrastructure.Repositories;

public class WateringRepository : IWateringRepository
{
    private readonly FloraLinkDbContext _db;

    public WateringRepository(FloraLinkDbContext db) => _db = db;

    public async Task<IEnumerable<WateringEvent>> GetByPlantIdAsync(int plantId) =>
        await _db.WateringEvents
            .Where(w => w.PlantId == plantId)
            .OrderByDescending(w => w.WateredAt)
            .ToListAsync();

    public Task<WateringEvent?> GetLastWateringAsync(int plantId) =>
        _db.WateringEvents
            .Where(w => w.PlantId == plantId)
            .OrderByDescending(w => w.WateredAt)
            .FirstOrDefaultAsync();

    public async Task<WateringEvent> AddAsync(WateringEvent wateringEvent)
    {
        _db.WateringEvents.Add(wateringEvent);
        await _db.SaveChangesAsync();
        return wateringEvent;
    }
}
