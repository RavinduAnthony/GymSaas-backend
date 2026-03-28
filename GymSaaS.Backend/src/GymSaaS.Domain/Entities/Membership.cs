namespace GymSaaS.Domain.Entities;

public class Membership : BaseTenantEntity
{
    public Guid MemberId { get; set; }
    public Guid PackageId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public string PaymentStatus { get; set; } = "Pending";

    // Navigation
    public Member Member { get; set; } = null!;
    public MembershipPackage Package { get; set; } = null!;
}
