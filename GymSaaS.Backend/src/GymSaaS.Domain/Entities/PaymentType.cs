namespace GymSaaS.Domain.Entities;

/// <summary>
/// Master/reference table for payment categories.
/// Seeded with default types; add new rows to extend without code changes.
/// </summary>
public class PaymentType
{
    public int Id { get; set; }

    /// <summary>Human-readable label, e.g. "Registration Fee"</summary>
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
}
