namespace GymSaaS.Application.DTOs.ServiceSettings;

public class ServiceSettingResponseDto
{
    public Guid Id { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public decimal DefaultAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateServiceSettingDto
{
    public decimal DefaultAmount { get; set; }
    public string? Notes { get; set; }
}
