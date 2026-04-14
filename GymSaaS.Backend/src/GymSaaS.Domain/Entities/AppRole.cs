namespace GymSaaS.Domain.Entities;

public class AppRole : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    /// <summary>
    /// For custom roles, the underlying system role used for backend [Authorize(Roles=...)] checks.
    /// Null for system roles (they are their own base).
    /// </summary>
    public string? BaseRole { get; set; }
    public string? Color { get; set; }
}
