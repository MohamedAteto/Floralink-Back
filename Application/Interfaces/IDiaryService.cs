using FloraLink.Application.DTOs.Diary;

namespace FloraLink.Application.Interfaces;

public interface IDiaryService
{
    Task<IEnumerable<DiaryEntryDto>> GetEntriesAsync(int plantId);
    Task<DiaryEntryDto> AddEntryAsync(CreateDiaryEntryDto dto);
    Task DeleteEntryAsync(int entryId);
}
