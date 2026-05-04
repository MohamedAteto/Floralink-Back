using FloraLink.Domain.Entities;

namespace FloraLink.Domain.Interfaces;

public interface ISensorReadingRepository
{
    Task<IEnumerable<SensorReading>> GetByPlantIdAsync(int plantId, int limit = 100);
    Task<SensorReading?> GetLatestByPlantIdAsync(int plantId);
    Task<SensorReading> AddAsync(SensorReading reading);
    Task<IEnumerable<SensorReading>> GetRecentAsync(int plantId, int days);
}
