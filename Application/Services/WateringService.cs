using FloraLink.Application.DTOs.Watering;
using FloraLink.Application.Interfaces;
using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;

namespace FloraLink.Application.Services;

public class WateringService : IWateringService
{
    private readonly IWateringRepository _watering;

    public WateringService(IWateringRepository watering)
    {
        _watering = watering;
    }

    public async Task<WateringDto> TriggerWateringAsync(TriggerWateringDto dto, bool isAutomatic)
    {
        var ev = new WateringEvent
        {
            PlantId = dto.PlantId,
            WaterAmountMl = dto.WaterAmountMl,
            IsAutomatic = isAutomatic,
            Notes = dto.Notes,
            WateredAt = DateTime.UtcNow
        };

        var saved = await _watering.AddAsync(ev);
        return MapToDto(saved);
    }

    public async Task<IEnumerable<WateringDto>> GetWateringHistoryAsync(int plantId)
    {
        var events = await _watering.GetByPlantIdAsync(plantId);
        return events.Select(MapToDto);
    }

    private static WateringDto MapToDto(WateringEvent e) => new()
    {
        Id = e.Id,
        PlantId = e.PlantId,
        WateredAt = e.WateredAt,
        WaterAmountMl = e.WaterAmountMl,
        IsAutomatic = e.IsAutomatic,
        Notes = e.Notes
    };
}
