namespace GymSaaS.Domain.Entities;

public class ClassType : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<GymClass> GymClasses { get; set; } = new List<GymClass>();
}
