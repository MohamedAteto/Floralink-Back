using FloraLink.Application.DTOs.Sensor;

namespace FloraLink.Application.Interfaces;

public interface ISensorService
{
    Task<SensorReadingDto> ProcessSensorDataAsync(SensorDataDto dto);
    Task<IEnumerable<SensorReadingDto>> GetReadingsAsync(int plantId, int limit = 100);
}
