namespace GymSaaS.Application.DTOs.Payments;

public class PaymentScheduleDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public Guid MembershipId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;       // Pending | Paid | Late
    public int PaymentTypeId { get; set; }
    public string PaymentTypeName { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
    public string Month { get; set; } = string.Empty;       // "2026-03"
}

public class RecordPaymentDto
{
    public Guid ScheduleId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
}

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int PaymentTypeId { get; set; }
    public string PaymentTypeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class PaymentDashboardSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public int PendingCount { get; set; }
    public decimal PendingAmount { get; set; }
    public int LateCount { get; set; }
    public decimal LateAmount { get; set; }
    public int PaidThisMonth { get; set; }
}

public class PaymentTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
