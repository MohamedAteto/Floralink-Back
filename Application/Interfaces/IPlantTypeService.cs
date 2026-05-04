using FloraLink.Domain.Entities;

namespace FloraLink.Application.Interfaces;

public interface IPlantTypeService
{
    Task<IEnumerable<PlantType>> GetAllAsync();
    Task<PlantType?> GetOrCreateByNameAsync(string name);
}
