namespace GymSaaS.Application.DTOs.Users;

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    /// <summary>Custom role name (must exist in AppRoles for this tenant).</summary>
    public string CustomRole { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? CustomRole { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? CustomRole { get; set; }
    public bool IsActive { get; set; }
    public bool IsTemporaryPassword { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Returned from POST /user — includes whether the welcome email was sent.</summary>
public class CreateUserResponseDto : UserResponseDto
{
    public bool EmailSent { get; set; }
    public string? EmailError { get; set; }
}
