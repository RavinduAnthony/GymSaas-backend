namespace GymSaaS.Domain.Entities;

public class MemberDeletionLog : BaseTenantEntity
{
    public Guid OriginalMemberId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime DeletedAt { get; set; }
    public string? Notes { get; set; }
}
