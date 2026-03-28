namespace GymSaaS.Domain.Entities;

public class Payment : BaseTenantEntity
{
    public Guid MemberId { get; set; }
    public Guid? MembershipId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string PlanName { get; set; } = string.Empty;

    // Navigation
    public Member Member { get; set; } = null!;
    public Membership? Membership { get; set; }
}
