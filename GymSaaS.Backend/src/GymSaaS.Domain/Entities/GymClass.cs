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
    public DateTime? BatchStartDate { get; set; }
    public DateTime? BatchEndDate { get; set; }

    // Capacity
    public int MaxCapacity { get; set; }

    // Pricing
    public decimal DefaultAmount { get; set; }
    public decimal HourlyRate { get; set; }

    // Location
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string Status { get; set; } = "Active";

    // Per-day schedules
    public ICollection<ClassSchedule> Schedules { get; set; } = new List<ClassSchedule>();
}
