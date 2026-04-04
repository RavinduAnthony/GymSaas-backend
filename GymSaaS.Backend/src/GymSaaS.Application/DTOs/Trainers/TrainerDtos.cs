namespace GymSaaS.Application.DTOs.Trainers;

public class CreateTrainerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Guid? TrainerTypeId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Certifications { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Availability { get; set; }
    // Set by controller after Cloudinary upload
    public string? PhotoUrl { get; set; }
}

public class UpdateTrainerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Guid? TrainerTypeId { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? DateOfBirth { get; set; }
    public string? Certifications { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Availability { get; set; }
    // Set by controller after Cloudinary upload (null = keep existing photo)
    public string? PhotoUrl { get; set; }
}

public class TrainerResponseDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public Guid? TrainerTypeId { get; set; }
    public string? TrainerTypeName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ExperienceYears { get; set; }
    public string? Certifications { get; set; }
    public string? Availability { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Age { get; set; }
    public string? Photo { get; set; }
    public DateTime CreatedAt { get; set; }
}
