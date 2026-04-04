using GymSaaS.Domain.Enums;

namespace GymSaaS.Domain.Entities;

public class ServiceSetting : BaseTenantEntity
{
    public ServiceType ServiceType { get; set; }
    public decimal DefaultAmount { get; set; } = 0;
    public string? Notes { get; set; }
}
