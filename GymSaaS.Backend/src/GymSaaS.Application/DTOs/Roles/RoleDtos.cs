namespace GymSaaS.Application.DTOs.Roles;

public class AppRoleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public string? BaseRole { get; set; }
    public string? Color { get; set; }
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class RolePermissionDto
{
    public string PermissionKey { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
}

public class UpdateRolePermissionsDto
{
    public List<RolePermissionDto> Permissions { get; set; } = new();
}
