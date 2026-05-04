using FloraLink.Application.DTOs.Diary;
using FloraLink.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloraLink.API.Controllers;

[ApiController]
[Route("api/diary")]
[Authorize]
public class DiaryController : ControllerBase
{
    private readonly IDiaryService _diary;

    public DiaryController(IDiaryService diary) => _diary = diary;

    [HttpGet("{plantId}")]
    public async Task<IActionResult> GetEntries(int plantId) =>
        Ok(await _diary.GetEntriesAsync(plantId));

    [HttpPost]
    public async Task<IActionResult> AddEntry([FromBody] CreateDiaryEntryDto dto)
    {
        var entry = await _diary.AddEntryAsync(dto);
        return Ok(entry);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        await _diary.DeleteEntryAsync(id);
        return NoContent();
    }
}
