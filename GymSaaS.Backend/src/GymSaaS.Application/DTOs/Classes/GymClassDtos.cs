namespace GymSaaS.Application.DTOs.Classes;

public class CreateGymClassDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public Guid? InstructorId { get; set; }
    public string? DaysOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime BatchStartDate { get; set; }
    public DateTime BatchEndDate { get; set; }
    public int MaxCapacity { get; set; }
    public decimal DefaultAmount { get; set; }
    public Guid? BranchId { get; set; }
    public string Status { get; set; } = "Active";
}

public class UpdateGymClassDto : CreateGymClassDto { }

public class GymClassResponseDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public Guid? InstructorId { get; set; }
    public string? InstructorName { get; set; }
    public string? InstructorPhone { get; set; }
    public string? InstructorEmail { get; set; }
    public string? InstructorPhoto { get; set; }
    public string? InstructorSpecialization { get; set; }
    public string? DaysOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime BatchStartDate { get; set; }
    public DateTime BatchEndDate { get; set; }
    public int MaxCapacity { get; set; }
    public decimal DefaultAmount { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
