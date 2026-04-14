namespace GymSaaS.Application.DTOs.PtRegistrations;

public class CreatePtRegistrationDto
{
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public decimal PaymentRatePerStudent { get; set; }
    public string Status { get; set; } = "Active";
}

public class UpdatePtRegistrationDto
{
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public decimal PaymentRatePerStudent { get; set; }
    public string Status { get; set; } = "Active";
}

public class PtRegistrationResponseDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public decimal PaymentRatePerStudent { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
