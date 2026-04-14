namespace GymSaaS.Domain.Entities;

public class RolePermission : BaseTenantEntity
{
    public string RoleName { get; set; } = string.Empty;
    public string PermissionKey { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
}
