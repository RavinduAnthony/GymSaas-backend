using GymSaaS.Application.DTOs.ServiceSettings;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Enums;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;

namespace GymSaaS.Application.Services;

public class ServiceSettingService
{
    private readonly IRepository<ServiceSetting> _repo;
    private readonly ITenantProvider _tenantProvider;

    public ServiceSettingService(IRepository<ServiceSetting> repo, ITenantProvider tenantProvider)
    {
        _repo = repo;
        _tenantProvider = tenantProvider;
    }

    public async Task<ApiResponse<IEnumerable<ServiceSettingResponseDto>>> GetAllAsync()
    {
        try
        {
            var existing = (await _repo.GetAllAsync()).ToList();
            var allTypes = Enum.GetValues<ServiceType>();

            bool changed = false;
            foreach (var type in allTypes)
            {
                if (!existing.Any(s => s.ServiceType == type))
                {
                    var newSetting = new ServiceSetting
                    {
                        TenantId = _tenantProvider.TenantId,
                        ServiceType = type,
                        DefaultAmount = 0,
                    };
                    await _repo.AddAsync(newSetting);
                    existing.Add(newSetting);
                    changed = true;
                }
            }

            if (changed)
                await _repo.SaveChangesAsync();

            return ApiResponse<IEnumerable<ServiceSettingResponseDto>>.Ok(
                existing.OrderBy(s => s.ServiceType).Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<ServiceSettingResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ServiceSettingResponseDto>> UpdateAsync(ServiceType serviceType, UpdateServiceSettingDto dto)
    {
        try
        {
            var settings = (await _repo.FindAsync(s => s.ServiceType == serviceType)).FirstOrDefault();

            if (settings == null)
            {
                settings = new ServiceSetting
                {
                    TenantId = _tenantProvider.TenantId,
                    ServiceType = serviceType,
                    DefaultAmount = dto.DefaultAmount,
                    Notes = dto.Notes,
                };
                await _repo.AddAsync(settings);
            }
            else
            {
                settings.DefaultAmount = dto.DefaultAmount;
                settings.Notes = dto.Notes;
                settings.UpdatedAt = DateTime.UtcNow;
                _repo.Update(settings);
            }

            await _repo.SaveChangesAsync();
            return ApiResponse<ServiceSettingResponseDto>.Ok(MapToDto(settings), "Service setting updated.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ServiceSettingResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    private static ServiceSettingResponseDto MapToDto(ServiceSetting s) => new()
    {
        Id = s.Id,
        ServiceType = s.ServiceType.ToString(),
        DefaultAmount = s.DefaultAmount,
        Notes = s.Notes,
        UpdatedAt = s.UpdatedAt,
    };
}
