using FloraLink.Domain.Entities;

namespace FloraLink.Domain.Interfaces;

public interface IPlantRepository
{
    Task<IEnumerable<Plant>> GetAllByUserIdAsync(int userId);
    Task<Plant?> GetByIdAsync(int id);
    Task<Plant> AddAsync(Plant plant);
    Task UpdateAsync(Plant plant);
    Task DeleteAsync(int id);
}
