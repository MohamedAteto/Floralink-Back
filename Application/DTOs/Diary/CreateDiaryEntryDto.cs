namespace FloraLink.Application.DTOs.Diary;

public class CreateDiaryEntryDto
{
    public int PlantId { get; set; }
    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }
}
