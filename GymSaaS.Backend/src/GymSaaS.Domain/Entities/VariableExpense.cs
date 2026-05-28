namespace GymSaaS.Domain.Entities;

/// <summary>
/// A one-off expense recorded for a specific month and year.
/// </summary>
public class VariableExpense : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int Month { get; set; }  // 1–12
    public int Year { get; set; }   // e.g. 2026
}
