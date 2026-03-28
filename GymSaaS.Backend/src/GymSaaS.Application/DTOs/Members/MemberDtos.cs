namespace GymSaaS.Application.DTOs.Members;

public class CreateMemberDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime JoinDate { get; set; }
    public Guid? BranchId { get; set; }
    public string? Email { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Address { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? MedicalConditions { get; set; }
    public Guid? TrainerId { get; set; }
}

public class UpdateMemberDto : CreateMemberDto
{
    public string Status { get; set; } = "Active";
}

public class MemberResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime JoinDate { get; set; }
    public Guid? BranchId { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Address { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? MedicalConditions { get; set; }
    public Guid? TrainerId { get; set; }
    public DateTime CreatedAt { get; set; }
}
