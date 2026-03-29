namespace GymSaaS.Domain.Entities;

public class MembershipPackage : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    /// <summary>"Monthly" = separate payment each month | "FullPayment" = single upfront payment for the full duration</summary>
    public string BillingFrequency { get; set; } = "Monthly";

    // Optional settings
    public string? Description { get; set; }
    public int? MaxVisits { get; set; }
    public bool TrainerIncluded { get; set; } = false;
    public int? FreezeDays { get; set; }
    public bool DiscountAllowed { get; set; } = false;

    // Navigation
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}
