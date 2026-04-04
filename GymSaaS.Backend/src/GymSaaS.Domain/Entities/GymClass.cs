namespace GymSaaS.Domain.Entities;

public class GymClass : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }

    // Instructor (FK → Trainer)
    public Guid? InstructorId { get; set; }
    public Trainer? Instructor { get; set; }

    // Schedule
    public string? DaysOfWeek { get; set; }   // e.g. "Mon,Wed,Fri"
    public string StartTime { get; set; } = string.Empty;  // e.g. "09:00"
    public string EndTime { get; set; } = string.Empty;    // e.g. "10:30"
    public int DurationMinutes { get; set; }               // auto-calculated
    public DateTime BatchStartDate { get; set; }
    public DateTime BatchEndDate { get; set; }

    // Capacity
    public int MaxCapacity { get; set; }

    // Pricing
    public decimal DefaultAmount { get; set; }

    // Location
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string Status { get; set; } = "Active";
}
