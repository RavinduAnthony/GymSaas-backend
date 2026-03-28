namespace GymSaaS.Domain.Entities;

public class TrainerAssignment : BaseTenantEntity
{
    public Guid TrainerId { get; set; }
    public Guid MemberId { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
}
