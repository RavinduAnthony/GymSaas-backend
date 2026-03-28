namespace GymSaaS.Domain.Entities;

public class Role : BaseTenantEntity
{
    public string RoleName { get; set; } = string.Empty;
}
