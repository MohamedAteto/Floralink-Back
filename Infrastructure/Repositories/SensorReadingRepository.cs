using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;
using FloraLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FloraLink.Infrastructure.Repositories;

public class SensorReadingRepository : ISensorReadingRepository
{
    private readonly FloraLinkDbContext _db;

    public SensorReadingRepository(FloraLinkDbContext db) => _db = db;

    public async Task<IEnumerable<SensorReading>> GetByPlantIdAsync(int plantId, int limit = 100) =>
        await _db.SensorReadings
            .Where(r => r.PlantId == plantId)
            .OrderByDescending(r => r.RecordedAt)
            .Take(limit)
            .ToListAsync();

    public Task<SensorReading?> GetLatestByPlantIdAsync(int plantId) =>
        _db.SensorReadings
            .Where(r => r.PlantId == plantId)
            .OrderByDescending(r => r.RecordedAt)
            .FirstOrDefaultAsync();

    public async Task<SensorReading> AddAsync(SensorReading reading)
    {
        _db.SensorReadings.Add(reading);
        await _db.SaveChangesAsync();
        return reading;
    }

    public async Task<IEnumerable<SensorReading>> GetRecentAsync(int plantId, int days) =>
        await _db.SensorReadings
            .Where(r => r.PlantId == plantId && r.RecordedAt >= DateTime.UtcNow.AddDays(-days))
            .OrderByDescending(r => r.RecordedAt)
            .ToListAsync();
}
