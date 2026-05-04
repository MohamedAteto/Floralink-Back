using FloraLink.Domain.Entities;

namespace FloraLink.Domain.Interfaces;

public interface IWateringRepository
{
    Task<IEnumerable<WateringEvent>> GetByPlantIdAsync(int plantId);
    Task<WateringEvent?> GetLastWateringAsync(int plantId);
    Task<WateringEvent> AddAsync(WateringEvent wateringEvent);
}
