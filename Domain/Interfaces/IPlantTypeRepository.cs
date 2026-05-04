using FloraLink.Domain.Entities;

namespace FloraLink.Domain.Interfaces;

public interface IPlantTypeRepository
{
    Task<IEnumerable<PlantType>> GetAllAsync();
    Task<PlantType?> GetByIdAsync(int id);
    Task<PlantType?> FindByNameAsync(string name);
    Task<PlantType> AddAsync(PlantType plantType);
}
