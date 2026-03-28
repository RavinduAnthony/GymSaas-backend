using GymSaaS.Application.DTOs.Tenant;

namespace GymSaaS.Application.Interfaces;

public interface ITenantService
{
    Task<Guid> RegisterTenantAsync(RegisterTenantRequest request);
}
