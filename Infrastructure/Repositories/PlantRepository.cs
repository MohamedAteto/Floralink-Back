using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;
using FloraLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FloraLink.Infrastructure.Repositories;

public class PlantRepository : IPlantRepository
{
    private readonly FloraLinkDbContext _db;

    public PlantRepository(FloraLinkDbContext db) => _db = db;

    public async Task<IEnumerable<Plant>> GetAllByUserIdAsync(int userId)
    {
        var query = _db.Plants.Include(p => p.PlantType).AsQueryable();
        if (userId != 0) query = query.Where(p => p.UserId == userId);
        return await query.ToListAsync();
    }

    public Task<Plant?> GetByIdAsync(int id) =>
        _db.Plants.Include(p => p.PlantType).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Plant> AddAsync(Plant plant)
    {
        _db.Plants.Add(plant);
        await _db.SaveChangesAsync();
        return plant;
    }

    public async Task UpdateAsync(Plant plant)
    {
        _db.Plants.Update(plant);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var plant = await _db.Plants.FindAsync(id);
        if (plant != null)
        {
            _db.Plants.Remove(plant);
            await _db.SaveChangesAsync();
        }
    }
}
