using GymSaaS.Domain.Interfaces;

namespace GymSaaS.Persistence;

public class TenantProvider : ITenantProvider
{
    public Guid TenantId { get; private set; }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
