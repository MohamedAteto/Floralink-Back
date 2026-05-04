using FloraLink.Application.DTOs.Plants;

namespace FloraLink.Application.Interfaces;

public interface IPlantService
{
    Task<IEnumerable<PlantDto>> GetUserPlantsAsync(int userId);
    Task<PlantDto?> GetPlantByIdAsync(int plantId, int userId);
    Task<PlantDto> CreatePlantAsync(CreatePlantDto dto, int userId);
    Task DeletePlantAsync(int plantId, int userId);
}
