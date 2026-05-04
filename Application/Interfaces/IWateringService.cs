using FloraLink.Application.DTOs.Watering;

namespace FloraLink.Application.Interfaces;

public interface IWateringService
{
    Task<WateringDto> TriggerWateringAsync(TriggerWateringDto dto, bool isAutomatic);
    Task<IEnumerable<WateringDto>> GetWateringHistoryAsync(int plantId);
}
