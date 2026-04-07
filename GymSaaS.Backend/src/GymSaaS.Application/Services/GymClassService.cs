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
    private readonly IRepository<ClassSchedule> _scheduleRepo;

    public GymClassService(IRepository<GymClass> repo, IRepository<ClassSchedule> scheduleRepo)
    {
        _repo = repo;
        _scheduleRepo = scheduleRepo;
    }

    public async Task<ApiResponse<IEnumerable<GymClassResponseDto>>> GetAllAsync()
    {
        var classes = await _repo.AsQueryable()
            .Include(c => c.Instructor)
            .Include(c => c.Branch)
            .Include(c => c.Schedules)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return ApiResponse<IEnumerable<GymClassResponseDto>>.Ok(classes.Select(MapToDto));
    }

    public async Task<ApiResponse<GymClassResponseDto>> GetByIdAsync(Guid id)
    {
        var gymClass = await _repo.AsQueryable()
            .Include(c => c.Instructor)
            .Include(c => c.Branch)
            .Include(c => c.Schedules)
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
            BatchStartDate = dto.BatchStartDate,
            BatchEndDate = dto.BatchEndDate,
            MaxCapacity = dto.MaxCapacity,
            DefaultAmount = dto.DefaultAmount,
            HourlyRate = dto.HourlyRate,
            BranchId = dto.BranchId,
            Status = dto.Status,
        };

        await _repo.AddAsync(gymClass);
        await _repo.SaveChangesAsync();

        await SyncSchedulesAsync(gymClass.Id, dto.BranchId, dto.TimeSlots);

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
        gymClass.BatchStartDate = dto.BatchStartDate;
        gymClass.BatchEndDate = dto.BatchEndDate;
        gymClass.MaxCapacity = dto.MaxCapacity;
        gymClass.DefaultAmount = dto.DefaultAmount;
        gymClass.HourlyRate = dto.HourlyRate;
        gymClass.BranchId = dto.BranchId;
        gymClass.Status = dto.Status;

        _repo.Update(gymClass);
        await _repo.SaveChangesAsync();

        await SyncSchedulesAsync(gymClass.Id, dto.BranchId, dto.TimeSlots);

        return await GetByIdAsync(gymClass.Id);
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var gymClass = await _repo.GetByIdAsync(id);
        if (gymClass is null)
            return ApiResponse.Fail("Class not found.");

        var schedules = await _scheduleRepo.AsQueryable()
            .Where(s => s.GymClassId == id)
            .ToListAsync();
        foreach (var s in schedules)
            _scheduleRepo.Delete(s);

        _repo.Delete(gymClass);
        await _repo.SaveChangesAsync();

        return ApiResponse.Ok("Class deleted successfully.");
    }

    // ─── Helpers ────────────────────────────────────────

    private async Task SyncSchedulesAsync(Guid classId, Guid? branchId, List<TimeSlotItemDto> inputSlots)
    {
        var existing = await _scheduleRepo.AsQueryable()
            .Where(s => s.GymClassId == classId)
            .ToListAsync();
        foreach (var s in existing)
            _scheduleRepo.Delete(s);

        if (branchId.HasValue && inputSlots.Count > 0)
        {
            // Aggregate individual 30-min slot picks into one per-day schedule record
            var perDay = inputSlots
                .GroupBy(s => s.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var (day, slots) in perDay)
            {
                await _scheduleRepo.AddAsync(new ClassSchedule
                {
                    GymClassId = classId,
                    BranchId = branchId.Value,
                    DayOfWeek = day,
                    StartTime = slots.Min(s => s.StartTime)!,
                    EndTime = slots.Max(s => s.EndTime)!,
                });
            }
        }

        await _scheduleRepo.SaveChangesAsync();
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
        BatchStartDate = c.BatchStartDate,
        BatchEndDate = c.BatchEndDate,
        MaxCapacity = c.MaxCapacity,
        DefaultAmount = c.DefaultAmount,
        HourlyRate = c.HourlyRate,
        BranchId = c.BranchId,
        BranchName = c.Branch?.Name,
        Status = c.Status,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Schedules = c.Schedules.Select(s => new ClassScheduleItemDto
        {
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
        }).ToList(),
    };
}

