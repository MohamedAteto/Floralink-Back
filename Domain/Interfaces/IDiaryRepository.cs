using FloraLink.Domain.Entities;

namespace FloraLink.Domain.Interfaces;

public interface IDiaryRepository
{
    Task<IEnumerable<PlantDiaryEntry>> GetByPlantIdAsync(int plantId);
    Task<PlantDiaryEntry> AddAsync(PlantDiaryEntry entry);
    Task DeleteAsync(int id);
}
