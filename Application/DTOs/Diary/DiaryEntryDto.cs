namespace FloraLink.Application.DTOs.Diary;

public class DiaryEntryDto
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime EntryDate { get; set; }
}
