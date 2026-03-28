namespace GymSaaS.Domain.Entities;

public class Member : BaseTenantEntity
{
    // Mandatory
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime JoinDate { get; set; }
    public Guid? BranchId { get; set; }
    public string Status { get; set; } = "Active";

    // Optional
    public string? Email { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Address { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? MedicalConditions { get; set; }
    public string? Photo { get; set; }

    // Navigation
    public Guid? TrainerId { get; set; }
    public Trainer? Trainer { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
