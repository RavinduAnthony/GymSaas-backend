namespace GymSaaS.Domain.Entities;

public class ClassSchedule : BaseTenantEntity
{
    public Guid GymClassId { get; set; }
    public GymClass GymClass { get; set; } = null!;

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public string DayOfWeek { get; set; } = string.Empty;  // e.g. "Monday"
    public string StartTime { get; set; } = string.Empty;  // e.g. "09:00"
    public string EndTime { get; set; } = string.Empty;    // e.g. "10:30"
}
