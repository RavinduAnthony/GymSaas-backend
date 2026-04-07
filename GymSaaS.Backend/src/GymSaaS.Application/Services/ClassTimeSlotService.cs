using GymSaaS.Application.DTOs.Classes;
using GymSaaS.Application.Interfaces;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class ClassTimeSlotService : IClassTimeSlotService
{
    private readonly IRepository<ClassSchedule> _repo;

    public ClassTimeSlotService(IRepository<ClassSchedule> repo)
    {
        _repo = repo;
    }

    public async Task<ApiResponse<IEnumerable<ClassTimeSlotResponseDto>>> GetByBranchAsync(
        Guid branchId, Guid? excludeClassId = null)
    {
        var query = _repo.AsQueryable()
            .Include(s => s.GymClass)
            .Where(s => s.BranchId == branchId);

        if (excludeClassId.HasValue)
            query = query.Where(s => s.GymClassId != excludeClassId.Value);

        var schedules = await query.ToListAsync();

        // Expand each per-day schedule into individual 30-min slots
        // so the frontend slot picker can mark occupied time windows
        var result = new List<ClassTimeSlotResponseDto>();
        foreach (var sched in schedules)
        {
            var cur = ParseMinutes(sched.StartTime);
            var end = ParseMinutes(sched.EndTime);
            while (cur + 30 <= end)
            {
                result.Add(new ClassTimeSlotResponseDto
                {
                    Id = sched.Id,
                    BranchId = sched.BranchId,
                    GymClassId = sched.GymClassId,
                    ClassName = sched.GymClass?.Name ?? string.Empty,
                    DayOfWeek = sched.DayOfWeek,
                    StartTime = FormatMinutes(cur),
                    EndTime = FormatMinutes(cur + 30),
                });
                cur += 30;
            }
        }

        return ApiResponse<IEnumerable<ClassTimeSlotResponseDto>>.Ok(result);
    }

    private static int ParseMinutes(string time)
    {
        var parts = time.Split(':');
        return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
    }

    private static string FormatMinutes(int minutes) =>
        $"{minutes / 60:D2}:{minutes % 60:D2}";
}
