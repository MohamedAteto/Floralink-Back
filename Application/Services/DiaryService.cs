using FloraLink.Application.DTOs.Diary;
using FloraLink.Application.Interfaces;
using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;

namespace FloraLink.Application.Services;

public class DiaryService : IDiaryService
{
    private readonly IDiaryRepository _diary;

    public DiaryService(IDiaryRepository diary)
    {
        _diary = diary;
    }

    public async Task<IEnumerable<DiaryEntryDto>> GetEntriesAsync(int plantId)
    {
        var entries = await _diary.GetByPlantIdAsync(plantId);
        return entries.Select(MapToDto);
    }

    public async Task<DiaryEntryDto> AddEntryAsync(CreateDiaryEntryDto dto)
    {
        var entry = new PlantDiaryEntry
        {
            PlantId = dto.PlantId,
            Notes = dto.Notes,
            PhotoUrl = dto.PhotoUrl,
            EntryDate = DateTime.UtcNow
        };
        var saved = await _diary.AddAsync(entry);
        return MapToDto(saved);
    }

    public async Task DeleteEntryAsync(int entryId)
    {
        await _diary.DeleteAsync(entryId);
    }

    private static DiaryEntryDto MapToDto(PlantDiaryEntry e) => new()
    {
        Id = e.Id,
        PlantId = e.PlantId,
        Notes = e.Notes,
        PhotoUrl = e.PhotoUrl,
        EntryDate = e.EntryDate
    };
}
