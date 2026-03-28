namespace GymSaaS.Domain.Entities;

public class Attendance : BaseTenantEntity
{
    public Guid MemberId { get; set; }
    public string BranchId { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }

    // Navigation
    public Member Member { get; set; } = null!;
}
