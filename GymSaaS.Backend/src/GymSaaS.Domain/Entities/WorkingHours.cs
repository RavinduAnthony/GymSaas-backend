namespace GymSaaS.Domain.Entities;

public class WorkingHours : BaseTenantEntity
{
    public string Day { get; set; } = string.Empty;       // "Monday", "Tuesday", etc.
    public string OpenTime { get; set; } = "05:00";
    public string CloseTime { get; set; } = "22:00";
    public bool IsClosed { get; set; } = false;
}
