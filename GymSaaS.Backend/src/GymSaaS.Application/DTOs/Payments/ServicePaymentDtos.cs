namespace GymSaaS.Application.DTOs.Payments;

public class ServicePaymentScheduleDto
{
    public Guid Id { get; set; }
    public string ServiceType { get; set; } = string.Empty;   // "Class" | "PT"
    public Guid? GymClassId { get; set; }
    public Guid? PtRegistrationId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;        // Pending | Paid | Late
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
}

public class RecordServicePaymentDto
{
    public Guid ScheduleId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
}

public class ServicePaymentHistoryDto
{
    public Guid Id { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class GenerateServiceSchedulesDto
{
    /// <summary>"Class" | "PT" | null (both)</summary>
    public string? ServiceType { get; set; }
}
