namespace GymSaaS.Application.DTOs.Packages;

public class CreatePackageDto
{
    public string Name { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MaxVisits { get; set; }
    public bool TrainerIncluded { get; set; }
    public int? FreezeDays { get; set; }
    public bool DiscountAllowed { get; set; }
    /// <summary>"Monthly" | "FullPayment"</summary>
    public string BillingFrequency { get; set; } = "Monthly";
}

public class UpdatePackageDto
{
    public string Name { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MaxVisits { get; set; }
    public bool TrainerIncluded { get; set; }
    public int? FreezeDays { get; set; }
    public bool DiscountAllowed { get; set; }
    /// <summary>"Monthly" | "FullPayment"</summary>
    public string BillingFrequency { get; set; } = "Monthly";
}

public class PackageResponseDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MaxVisits { get; set; }
    public bool TrainerIncluded { get; set; }
    public int? FreezeDays { get; set; }
    public bool DiscountAllowed { get; set; }
    /// <summary>"Monthly" | "FullPayment"</summary>
    public string BillingFrequency { get; set; } = "Monthly";
    public DateTime CreatedAt { get; set; }
}
