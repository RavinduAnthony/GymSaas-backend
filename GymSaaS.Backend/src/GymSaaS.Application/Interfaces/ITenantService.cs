using GymSaaS.Application.DTOs.Tenant;

namespace GymSaaS.Application.Interfaces;

public interface ITenantService
{
    Task<Guid> RegisterTenantAsync(RegisterTenantRequest request);
    Task UpdateLogoAsync(Guid tenantId, string logoUrl);
    Task<string?> GetLogoUrlAsync(Guid tenantId);
    Task ClearLogoAsync(Guid tenantId);
}
