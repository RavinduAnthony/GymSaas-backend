namespace GymSaaS.Domain.Entities;

public class AuditLog : BaseTenantEntity
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
