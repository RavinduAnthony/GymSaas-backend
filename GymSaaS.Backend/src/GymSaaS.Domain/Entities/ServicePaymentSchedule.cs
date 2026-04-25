namespace GymSaaS.Domain.Entities;

/// <summary>
/// Monthly payment schedule for a Gym Class or PT Registration.
/// Schedules are generated on the last week of each month for the current month's services.
/// If payment is not received during the last week of the month it is considered late.
/// A payment received in the 1st week of the NEXT month is treated as a LATE payment
/// for the PREVIOUS month.
/// </summary>
public class ServicePaymentSchedule : BaseTenantEntity
{
    /// <summary>Discriminator: "Class" | "PT"</summary>
    public string ServiceType { get; set; } = string.Empty;

    /// <summary>FK → GymClass.Id  (null when ServiceType = "PT")</summary>
    public Guid? GymClassId { get; set; }
    public GymClass? GymClass { get; set; }

    /// <summary>FK → PtRegistration.Id  (null when ServiceType = "Class")</summary>
    public Guid? PtRegistrationId { get; set; }
    public PtRegistration? PtRegistration { get; set; }

    /// <summary>Human-readable name: class name or trainer name.</summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>The billing month this schedule belongs to, e.g. "2026-04".</summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>Due date = last day of the billing month (last week starts 7 days before end).</summary>
    public DateTime DueDate { get; set; }

    public decimal Amount { get; set; }

    /// <summary>Pending | Paid | Late</summary>
    public string Status { get; set; } = "Pending";

    public DateTime? PaidDate { get; set; }

    public string? Notes { get; set; }

    // Navigation
    public ICollection<ServicePayment> Payments { get; set; } = new List<ServicePayment>();
}
