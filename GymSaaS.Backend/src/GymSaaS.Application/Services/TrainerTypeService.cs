using GymSaaS.Application.DTOs.TrainerTypes;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;

namespace GymSaaS.Application.Services;

public class TrainerTypeService
{
    private readonly IRepository<TrainerType> _trainerTypeRepo;
    private readonly IRepository<Trainer> _trainerRepo;
    private readonly ITenantProvider _tenantProvider;

    public TrainerTypeService(
        IRepository<TrainerType> trainerTypeRepo,
        IRepository<Trainer> trainerRepo,
        ITenantProvider tenantProvider)
    {
        _trainerTypeRepo = trainerTypeRepo;
        _trainerRepo = trainerRepo;
        _tenantProvider = tenantProvider;
    }

    public async Task<ApiResponse<IEnumerable<TrainerTypeResponseDto>>> GetAllAsync()
    {
        try
        {
            var types = await _trainerTypeRepo.GetAllAsync();
            return ApiResponse<IEnumerable<TrainerTypeResponseDto>>.Ok(types.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<TrainerTypeResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrainerTypeResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var trainerType = await _trainerTypeRepo.GetByIdAsync(id);
            if (trainerType == null) return ApiResponse<TrainerTypeResponseDto>.Fail("Trainer type not found.");
            return ApiResponse<TrainerTypeResponseDto>.Ok(MapToDto(trainerType));
        }
        catch (Exception ex)
        {
            return ApiResponse<TrainerTypeResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrainerTypeResponseDto>> CreateAsync(CreateTrainerTypeDto dto)
    {
        try
        {
            var trainerType = new TrainerType
            {
                TenantId = _tenantProvider.TenantId,
                Name = dto.Name,
                Description = dto.Description,
            };

            await _trainerTypeRepo.AddAsync(trainerType);
            await _trainerTypeRepo.SaveChangesAsync();

            return ApiResponse<TrainerTypeResponseDto>.Ok(MapToDto(trainerType), "Trainer type created.");
        }
        catch (Exception ex)
        {
            return ApiResponse<TrainerTypeResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrainerTypeResponseDto>> UpdateAsync(Guid id, UpdateTrainerTypeDto dto)
    {
        try
        {
            var trainerType = await _trainerTypeRepo.GetByIdAsync(id);
            if (trainerType == null) return ApiResponse<TrainerTypeResponseDto>.Fail("Trainer type not found.");

            trainerType.Name = dto.Name;
            trainerType.Description = dto.Description;
            trainerType.IsActive = dto.IsActive;

            _trainerTypeRepo.Update(trainerType);
            await _trainerTypeRepo.SaveChangesAsync();

            return ApiResponse<TrainerTypeResponseDto>.Ok(MapToDto(trainerType), "Trainer type updated.");
        }
        catch (Exception ex)
        {
            return ApiResponse<TrainerTypeResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        try
        {
            var trainerType = await _trainerTypeRepo.GetByIdAsync(id);
            if (trainerType == null) return ApiResponse.Fail("Trainer type not found.");

            var assignedTrainers = await _trainerRepo.FindAsync(t => t.TrainerTypeId == id);
            if (assignedTrainers.Any())
                return ApiResponse.Fail($"Cannot delete '{trainerType.Name}' — it is assigned to {assignedTrainers.Count()} trainer(s). Reassign them first.");

            _trainerTypeRepo.Delete(trainerType);
            await _trainerTypeRepo.SaveChangesAsync();

            return ApiResponse.Ok("Trainer type deleted.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"An error occurred: {ex.Message}");
        }
    }

    private static TrainerTypeResponseDto MapToDto(TrainerType tt) => new()
    {
        Id = tt.Id,
        TenantId = tt.TenantId,
        Name = tt.Name,
        Description = tt.Description,
        IsActive = tt.IsActive,
        CreatedAt = tt.CreatedAt,
    };
}
