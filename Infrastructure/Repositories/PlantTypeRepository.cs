using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;
using FloraLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FloraLink.Infrastructure.Repositories;

public class PlantTypeRepository : IPlantTypeRepository
{
    private readonly FloraLinkDbContext _db;

    public PlantTypeRepository(FloraLinkDbContext db) => _db = db;

    public async Task<IEnumerable<PlantType>> GetAllAsync() =>
        await _db.PlantTypes.ToListAsync();

    public Task<PlantType?> GetByIdAsync(int id) =>
        _db.PlantTypes.FindAsync(id).AsTask();

    public Task<PlantType?> FindByNameAsync(string name) =>
        _db.PlantTypes.FirstOrDefaultAsync(p =>
            p.Name.ToLower() == name.ToLower());

    public async Task<PlantType> AddAsync(PlantType plantType)
    {
        _db.PlantTypes.Add(plantType);
        await _db.SaveChangesAsync();
        return plantType;
    }
}
