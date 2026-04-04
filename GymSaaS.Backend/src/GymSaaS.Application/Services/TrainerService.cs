using GymSaaS.Application.DTOs.Trainers;
using GymSaaS.Application.Interfaces;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class TrainerService
{
    private readonly IRepository<Trainer> _trainerRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICloudinaryService _cloudinaryService;

    public TrainerService(
        IRepository<Trainer> trainerRepo,
        ITenantProvider tenantProvider,
        ICloudinaryService cloudinaryService)
    {
        _trainerRepo = trainerRepo;
        _tenantProvider = tenantProvider;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<ApiResponse<IEnumerable<TrainerResponseDto>>> GetAllAsync()
    {
        try
        {
            var trainers = await _trainerRepo.AsQueryable()
                .Include(t => t.TrainerType)
                .Include(t => t.Branch)
                .ToListAsync();
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
            var trainer = await _trainerRepo.AsQueryable()
                .Include(t => t.TrainerType)
                .Include(t => t.Branch)
                .FirstOrDefaultAsync(t => t.Id == id);
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
                TrainerTypeId = dto.TrainerTypeId,
                DateOfBirth = dto.DateOfBirth,
                Age = CalculateAge(dto.DateOfBirth),
                Photo = dto.PhotoUrl,
                Certifications = dto.Certifications,
                ExperienceYears = dto.ExperienceYears,
                Availability = dto.Availability,
            };

            await _trainerRepo.AddAsync(trainer);
            await _trainerRepo.SaveChangesAsync();

            var created = await _trainerRepo.AsQueryable()
                .Include(t => t.TrainerType)
                .Include(t => t.Branch)
                .FirstOrDefaultAsync(t => t.Id == trainer.Id);

            return ApiResponse<TrainerResponseDto>.Ok(MapToDto(created!), "Trainer created.");
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
            trainer.TrainerTypeId = dto.TrainerTypeId;
            trainer.Status = dto.Status;
            trainer.DateOfBirth = dto.DateOfBirth;
            trainer.Age = CalculateAge(dto.DateOfBirth);
            if (dto.PhotoUrl != null)
                trainer.Photo = dto.PhotoUrl;
            trainer.Certifications = dto.Certifications;
            trainer.ExperienceYears = dto.ExperienceYears;
            trainer.Availability = dto.Availability;

            _trainerRepo.Update(trainer);
            await _trainerRepo.SaveChangesAsync();

            var updated = await _trainerRepo.AsQueryable()
                .Include(t => t.TrainerType)
                .Include(t => t.Branch)
                .FirstOrDefaultAsync(t => t.Id == id);

            return ApiResponse<TrainerResponseDto>.Ok(MapToDto(updated!), "Trainer updated.");
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

    public async Task<ApiResponse> DeletePhotoAsync(Guid id)
    {
        try
        {
            var trainer = await _trainerRepo.GetByIdAsync(id);
            if (trainer == null) return ApiResponse.Fail("Trainer not found.");

            if (!string.IsNullOrEmpty(trainer.Photo))
            {
                await _cloudinaryService.DeleteImageByUrlAsync(trainer.Photo);
                trainer.Photo = null;
                _trainerRepo.Update(trainer);
                await _trainerRepo.SaveChangesAsync();
            }

            return ApiResponse.Ok("Trainer photo deleted.");
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
        TrainerTypeId = t.TrainerTypeId,
        TrainerTypeName = t.TrainerType?.Name,
        Status = t.Status,
        ExperienceYears = t.ExperienceYears,
        Certifications = t.Certifications,
        Availability = t.Availability,
        DateOfBirth = t.DateOfBirth,
        Age = CalculateAge(t.DateOfBirth),
        Photo = t.Photo,
        CreatedAt = t.CreatedAt,
    };

    private static string? CalculateAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return null;

        var today = DateTime.Today;
        var dob = dateOfBirth.Value.Date;

        var years = today.Year - dob.Year;
        var months = today.Month - dob.Month;

        if (today.Day < dob.Day)
            months--;

        if (months < 0)
        {
            years--;
            months += 12;
        }

        return $"{years} year{(years != 1 ? "s" : "")}, {months} month{(months != 1 ? "s" : "")}";
    }
}
