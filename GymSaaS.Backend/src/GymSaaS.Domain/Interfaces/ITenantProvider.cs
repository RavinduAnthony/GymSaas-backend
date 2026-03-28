namespace GymSaaS.Domain.Interfaces;

public interface ITenantProvider
{
    Guid TenantId { get; }
    void SetTenantId(Guid tenantId);
}
