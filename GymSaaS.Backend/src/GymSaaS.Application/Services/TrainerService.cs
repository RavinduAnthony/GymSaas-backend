using GymSaaS.Application.DTOs.Trainers;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class TrainerService
{
    private readonly IRepository<Trainer> _trainerRepo;
    private readonly ITenantProvider _tenantProvider;

    public TrainerService(IRepository<Trainer> trainerRepo, ITenantProvider tenantProvider)
    {
        _trainerRepo = trainerRepo;
        _tenantProvider = tenantProvider;
    }

    public async Task<ApiResponse<IEnumerable<TrainerResponseDto>>> GetAllAsync()
    {
        try
        {
            var trainers = await _trainerRepo.GetAllAsync();
            // Eager load Branch navigation for name resolution — if repo supports queryable, use Include.
            return ApiResponse<IEnumerable<TrainerResponseDto>>.Ok(trainers.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<TrainerResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrainerResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var trainer = await _trainerRepo.GetByIdAsync(id);
            if (trainer == null) return ApiResponse<TrainerResponseDto>.Fail("Trainer not found.");
            return ApiResponse<TrainerResponseDto>.Ok(MapToDto(trainer));
        }
        catch (Exception ex)
        {
            return ApiResponse<TrainerResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrainerResponseDto>> CreateAsync(CreateTrainerDto dto)
    {
        try
        {
            var trainer = new Trainer
            {
                TenantId = _tenantProvider.TenantId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Email = dto.Email,
                Specialization = dto.Specialization,
                BranchId = dto.BranchId,
                DateOfBirth = dto.DateOfBirth,
                Certifications = dto.Certifications,
                ExperienceYears = dto.ExperienceYears,
                Availability = dto.Availability,
            };

            await _trainerRepo.AddAsync(trainer);
            await _trainerRepo.SaveChangesAsync();

            return ApiResponse<TrainerResponseDto>.Ok(MapToDto(trainer), "Trainer created.");
        }
        catch (Exception ex)
        {
            return ApiResponse<TrainerResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrainerResponseDto>> UpdateAsync(Guid id, UpdateTrainerDto dto)
    {
        try
        {
            var trainer = await _trainerRepo.GetByIdAsync(id);
            if (trainer == null) return ApiResponse<TrainerResponseDto>.Fail("Trainer not found.");

            trainer.FirstName = dto.FirstName;
            trainer.LastName = dto.LastName;
            trainer.Phone = dto.Phone;
            trainer.Email = dto.Email;
            trainer.Specialization = dto.Specialization;
            trainer.BranchId = dto.BranchId;
            trainer.Status = dto.Status;
            trainer.DateOfBirth = dto.DateOfBirth;
            trainer.Certifications = dto.Certifications;
            trainer.ExperienceYears = dto.ExperienceYears;
            trainer.Availability = dto.Availability;

            _trainerRepo.Update(trainer);
            await _trainerRepo.SaveChangesAsync();

            return ApiResponse<TrainerResponseDto>.Ok(MapToDto(trainer), "Trainer updated.");
        }
        catch (Exception ex)
        {
            return ApiResponse<TrainerResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        try
        {
            var trainer = await _trainerRepo.GetByIdAsync(id);
            if (trainer == null) return ApiResponse.Fail("Trainer not found.");

            _trainerRepo.Delete(trainer);
            await _trainerRepo.SaveChangesAsync();

            return ApiResponse.Ok("Trainer deleted.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"An error occurred: {ex.Message}");
        }
    }

    private static TrainerResponseDto MapToDto(Trainer t) => new()
    {
        Id = t.Id,
        FirstName = t.FirstName,
        LastName = t.LastName,
        Phone = t.Phone,
        Email = t.Email,
        Specialization = t.Specialization,
        Branch = t.Branch?.Name ?? string.Empty,
        BranchId = t.BranchId,
        BranchName = t.Branch?.Name,
        Status = t.Status,
        ExperienceYears = t.ExperienceYears,
        Certifications = t.Certifications,
        Availability = t.Availability,
        DateOfBirth = t.DateOfBirth,
        CreatedAt = t.CreatedAt,
    };
}
