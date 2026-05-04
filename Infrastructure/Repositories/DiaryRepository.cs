using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;
using FloraLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FloraLink.Infrastructure.Repositories;

public class DiaryRepository : IDiaryRepository
{
    private readonly FloraLinkDbContext _db;

    public DiaryRepository(FloraLinkDbContext db) => _db = db;

    public async Task<IEnumerable<PlantDiaryEntry>> GetByPlantIdAsync(int plantId) =>
        await _db.PlantDiaryEntries
            .Where(d => d.PlantId == plantId)
            .OrderByDescending(d => d.EntryDate)
            .ToListAsync();

    public async Task<PlantDiaryEntry> AddAsync(PlantDiaryEntry entry)
    {
        _db.PlantDiaryEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _db.PlantDiaryEntries.FindAsync(id);
        if (entry != null)
        {
            _db.PlantDiaryEntries.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
