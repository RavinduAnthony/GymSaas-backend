namespace GymSaaS.Application.DTOs.Memberships;

public class CreateMembershipDto
{
    public Guid MemberId { get; set; }
    public Guid PackageId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    /// <summary>One-time registration fee (0 if waived).</summary>
    public decimal RegistrationFee { get; set; } = 0;
}

public class UpdateMembershipDto
{
    public Guid PackageId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
}

public class RenewMembershipDto
{
    public Guid MembershipId { get; set; }
    public DateTime NewEndDate { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
}

public class MembershipResponseDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string PlanName { get; set; } = string.Empty;
}
