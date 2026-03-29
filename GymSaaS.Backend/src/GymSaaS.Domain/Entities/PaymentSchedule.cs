namespace GymSaaS.Domain.Entities;

/// <summary>
/// Represents a single payment obligation for a membership.
/// Generated when a membership is created/renewed.
/// </summary>
public class PaymentSchedule : BaseTenantEntity
{
    public Guid MembershipId { get; set; }
    public Guid MemberId { get; set; }

    /// <summary>Due on the 5th of each month.</summary>
    public DateTime DueDate { get; set; }

    public decimal Amount { get; set; }

    /// <summary>Pending | Paid | Late</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>FK → PaymentTypes master table (e.g. 3 = RegularMonthly)</summary>
    public int PaymentTypeId { get; set; }

    /// <summary>When the payment was actually recorded.</summary>
    public DateTime? PaidDate { get; set; }

    public string? Notes { get; set; }

    /// <summary>ISO month string e.g. "2026-03"</summary>
    public string Month { get; set; } = string.Empty;

    // Navigation
    public Membership Membership { get; set; } = null!;
    public Member Member { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public PaymentType? PaymentType { get; set; }
}
