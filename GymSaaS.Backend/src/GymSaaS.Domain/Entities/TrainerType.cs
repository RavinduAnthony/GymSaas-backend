namespace GymSaaS.Domain.Entities;

public class TrainerType : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
}
