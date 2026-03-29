namespace GymSaaS.Domain.Entities;

public class Payment : BaseTenantEntity
{
    public Guid MemberId { get; set; }
    public Guid? MembershipId { get; set; }
    public Guid? PaymentScheduleId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string PlanName { get; set; } = string.Empty;

    /// <summary>FK → PaymentTypes master table (e.g. 3 = RegularMonthly)</summary>
    public int PaymentTypeId { get; set; }

    /// <summary>Paid | Late</summary>
    public string Status { get; set; } = "Paid";

    /// <summary>Cash | Card | BankTransfer</summary>
    public string Method { get; set; } = "Cash";

    public string? Notes { get; set; }

    // Navigation
    public Member Member { get; set; } = null!;
    public Membership? Membership { get; set; }
    public PaymentSchedule? PaymentSchedule { get; set; }
    public PaymentType? PaymentType { get; set; }
}
