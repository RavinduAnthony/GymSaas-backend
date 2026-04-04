using GymSaaS.Application.DTOs.Classes;
using GymSaaS.Application.Interfaces;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class GymClassService : IGymClassService
{
    private readonly IRepository<GymClass> _repo;

    public GymClassService(IRepository<GymClass> repo)
    {
        _repo = repo;
    }

    public async Task<ApiResponse<IEnumerable<GymClassResponseDto>>> GetAllAsync()
    {
        var classes = await _repo.AsQueryable()
            .Include(c => c.Instructor)
            .Include(c => c.Branch)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return ApiResponse<IEnumerable<GymClassResponseDto>>.Ok(classes.Select(MapToDto));
    }

    public async Task<ApiResponse<GymClassResponseDto>> GetByIdAsync(Guid id)
    {
        var gymClass = await _repo.AsQueryable()
            .Include(c => c.Instructor)
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (gymClass is null)
            return ApiResponse<GymClassResponseDto>.Fail("Class not found.");

        return ApiResponse<GymClassResponseDto>.Ok(MapToDto(gymClass));
    }

    public async Task<ApiResponse<GymClassResponseDto>> CreateAsync(CreateGymClassDto dto)
    {
        var gymClass = new GymClass
        {
            Name = dto.Name,
            Category = dto.Category,
            Description = dto.Description,
            InstructorId = dto.InstructorId,
            DaysOfWeek = dto.DaysOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            DurationMinutes = dto.DurationMinutes,
            BatchStartDate = dto.BatchStartDate,
            BatchEndDate = dto.BatchEndDate,
            MaxCapacity = dto.MaxCapacity,
            DefaultAmount = dto.DefaultAmount,
            BranchId = dto.BranchId,
            Status = dto.Status,
        };

        await _repo.AddAsync(gymClass);
        await _repo.SaveChangesAsync();

        return await GetByIdAsync(gymClass.Id);
    }

    public async Task<ApiResponse<GymClassResponseDto>> UpdateAsync(Guid id, UpdateGymClassDto dto)
    {
        var gymClass = await _repo.GetByIdAsync(id);
        if (gymClass is null)
            return ApiResponse<GymClassResponseDto>.Fail("Class not found.");

        gymClass.Name = dto.Name;
        gymClass.Category = dto.Category;
        gymClass.Description = dto.Description;
        gymClass.InstructorId = dto.InstructorId;
        gymClass.DaysOfWeek = dto.DaysOfWeek;
        gymClass.StartTime = dto.StartTime;
        gymClass.EndTime = dto.EndTime;
        gymClass.DurationMinutes = dto.DurationMinutes;
        gymClass.BatchStartDate = dto.BatchStartDate;
        gymClass.BatchEndDate = dto.BatchEndDate;
        gymClass.MaxCapacity = dto.MaxCapacity;
        gymClass.DefaultAmount = dto.DefaultAmount;
        gymClass.BranchId = dto.BranchId;
        gymClass.Status = dto.Status;

        _repo.Update(gymClass);
        await _repo.SaveChangesAsync();

        return await GetByIdAsync(gymClass.Id);
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var gymClass = await _repo.GetByIdAsync(id);
        if (gymClass is null)
            return ApiResponse.Fail("Class not found.");

        _repo.Delete(gymClass);
        await _repo.SaveChangesAsync();

        return ApiResponse.Ok("Class deleted successfully.");
    }

    private static GymClassResponseDto MapToDto(GymClass c) => new()
    {
        Id = c.Id,
        TenantId = c.TenantId,
        Name = c.Name,
        Category = c.Category,
        Description = c.Description,
        InstructorId = c.InstructorId,
        InstructorName = c.Instructor != null ? $"{c.Instructor.FirstName} {c.Instructor.LastName}" : null,
        InstructorPhone = c.Instructor?.Phone,
        InstructorEmail = c.Instructor?.Email,
        InstructorPhoto = c.Instructor?.Photo,
        InstructorSpecialization = c.Instructor?.Specialization,
        DaysOfWeek = c.DaysOfWeek,
        StartTime = c.StartTime,
        EndTime = c.EndTime,
        DurationMinutes = c.DurationMinutes,
        BatchStartDate = c.BatchStartDate,
        BatchEndDate = c.BatchEndDate,
        MaxCapacity = c.MaxCapacity,
        DefaultAmount = c.DefaultAmount,
        BranchId = c.BranchId,
        BranchName = c.Branch?.Name,
        Status = c.Status,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };
}
