namespace GymSaaS.Domain.Entities;

/// <summary>
/// A recurring expense that is automatically applied to every month's revenue calculation.
/// </summary>
public class FixedExpense : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
