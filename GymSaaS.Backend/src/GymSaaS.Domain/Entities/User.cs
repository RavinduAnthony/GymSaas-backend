using GymSaaS.Domain.Enums;

namespace GymSaaS.Domain.Entities;

public class User : BaseTenantEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public UserRole Role { get; set; } = UserRole.Receptionist;
    /// <summary>Custom role name if the user has been assigned a tenant-defined role.</summary>
    public string? CustomRole { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTemporaryPassword { get; set; } = false;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
