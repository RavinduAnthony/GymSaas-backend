namespace GymSaaS.Domain.Entities;

public class PtRegistration : BaseTenantEntity
{
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public decimal PaymentRatePerStudent { get; set; }
    public string Status { get; set; } = "Active";

    // Navigation
    public Trainer? Trainer { get; set; }
}
