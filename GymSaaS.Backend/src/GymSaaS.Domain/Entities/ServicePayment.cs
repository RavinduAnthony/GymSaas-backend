namespace GymSaaS.Domain.Entities;

/// <summary>
/// An actual payment recorded against a ServicePaymentSchedule.
/// </summary>
public class ServicePayment : BaseTenantEntity
{
    public Guid ServicePaymentScheduleId { get; set; }
    public ServicePaymentSchedule Schedule { get; set; } = null!;

    /// <summary>Mirror of schedule.ServiceType for quick filtering.</summary>
    public string ServiceType { get; set; } = string.Empty;

    /// <summary>Mirror of schedule.ServiceName for quick display.</summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>Mirror of schedule.Month (billing month) for quick display.</summary>
    public string Month { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    /// <summary>Paid | Late</summary>
    public string Status { get; set; } = "Paid";

    /// <summary>Cash | Card | BankTransfer</summary>
    public string Method { get; set; } = "Cash";

    public string? Notes { get; set; }
}
