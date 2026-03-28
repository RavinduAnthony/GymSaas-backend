using GymSaaS.Domain.Interfaces;

namespace GymSaaS.API.Middleware;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ITenantProvider tenantProvider)
    {
        var tenantClaim = context.User.FindFirst("TenantId")?.Value;

        if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var tenantId))
        {
            tenantProvider.SetTenantId(tenantId);
        }

        await _next(context);
    }
}
