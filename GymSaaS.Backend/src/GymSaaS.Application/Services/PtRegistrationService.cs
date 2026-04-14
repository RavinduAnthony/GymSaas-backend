using GymSaaS.Application.DTOs.PtRegistrations;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class PtRegistrationService
{
    private readonly IRepository<PtRegistration> _repo;
    private readonly ITenantProvider _tenantProvider;

    public PtRegistrationService(IRepository<PtRegistration> repo, ITenantProvider tenantProvider)
    {
        _repo = repo;
        _tenantProvider = tenantProvider;
    }

    public async Task<ApiResponse<IEnumerable<PtRegistrationResponseDto>>> GetAllAsync()
    {
        var records = await _repo.AsQueryable()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return ApiResponse<IEnumerable<PtRegistrationResponseDto>>.Ok(records.Select(MapToDto));
    }

    public async Task<ApiResponse<PtRegistrationResponseDto>> GetByIdAsync(Guid id)
    {
        var record = await _repo.GetByIdAsync(id);
        if (record == null)
            return ApiResponse<PtRegistrationResponseDto>.Fail("PT registration not found.");
        return ApiResponse<PtRegistrationResponseDto>.Ok(MapToDto(record));
    }

    public async Task<ApiResponse<PtRegistrationResponseDto>> CreateAsync(CreatePtRegistrationDto dto)
    {
        var entity = new PtRegistration
        {
            TenantId = _tenantProvider.TenantId,
            TrainerId = dto.TrainerId,
            TrainerName = dto.TrainerName,
            StudentCount = dto.StudentCount,
            PaymentRatePerStudent = dto.PaymentRatePerStudent,
            Status = dto.Status,
        };
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return ApiResponse<PtRegistrationResponseDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<PtRegistrationResponseDto>> UpdateAsync(Guid id, UpdatePtRegistrationDto dto)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<PtRegistrationResponseDto>.Fail("PT registration not found.");

        entity.TrainerId = dto.TrainerId;
        entity.TrainerName = dto.TrainerName;
        entity.StudentCount = dto.StudentCount;
        entity.PaymentRatePerStudent = dto.PaymentRatePerStudent;
        entity.Status = dto.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        _repo.Update(entity);
        await _repo.SaveChangesAsync();
        return ApiResponse<PtRegistrationResponseDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<bool>.Fail("PT registration not found.");

        _repo.Delete(entity);
        await _repo.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    private static PtRegistrationResponseDto MapToDto(PtRegistration r) => new()
    {
        Id = r.Id,
        TenantId = r.TenantId,
        TrainerId = r.TrainerId,
        TrainerName = r.TrainerName,
        StudentCount = r.StudentCount,
        PaymentRatePerStudent = r.PaymentRatePerStudent,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
    };
}
