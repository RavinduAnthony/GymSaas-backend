namespace GymSaaS.Domain.Entities;

public class Trainer : BaseTenantEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string Status { get; set; } = "Active";

    // Optional
    public DateTime? DateOfBirth { get; set; }
    public string? Age { get; set; }
    public string? Photo { get; set; }
    public string? Certifications { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Availability { get; set; }

    // Trainer Type
    public Guid? TrainerTypeId { get; set; }

    // Navigation
    public Branch? Branch { get; set; }
    public TrainerType? TrainerType { get; set; }
    public ICollection<Member> Members { get; set; } = new List<Member>();
}
