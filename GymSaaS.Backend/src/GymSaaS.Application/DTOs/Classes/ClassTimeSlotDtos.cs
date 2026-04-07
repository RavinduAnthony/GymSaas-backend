namespace GymSaaS.Application.DTOs.Classes;

public class ClassTimeSlotResponseDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid GymClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}
