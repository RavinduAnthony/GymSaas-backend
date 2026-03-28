namespace GymSaaS.Domain.Entities;

public class Branch : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Member> Members { get; set; } = new List<Member>();
    public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
}
