namespace GymSaaS.Application.DTOs.Users;

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    /// <summary>System role name: Owner | Manager | Receptionist | Trainer</summary>
    public string Role { get; set; } = "Receptionist";
    /// <summary>Optional custom role name (must exist in AppRoles).</summary>
    public string? CustomRole { get; set; }
}

public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? CustomRole { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? CustomRole { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
