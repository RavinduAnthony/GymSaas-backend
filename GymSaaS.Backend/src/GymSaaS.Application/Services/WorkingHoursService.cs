using GymSaaS.Application.DTOs.Settings;
using GymSaaS.Domain.Entities;
using GymSaaS.Persistence;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class WorkingHoursService
{
    private readonly GymDbContext _context;

    private static readonly string[] DayOrder =
        ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    private static readonly Dictionary<string, (string Open, string Close)> Defaults = new()
    {
        ["Monday"]    = ("05:00", "22:00"),
        ["Tuesday"]   = ("05:00", "22:00"),
        ["Wednesday"] = ("05:00", "22:00"),
        ["Thursday"]  = ("05:00", "22:00"),
        ["Friday"]    = ("05:00", "22:00"),
        ["Saturday"]  = ("05:00", "22:00"),
        ["Sunday"]    = ("08:00", "14:00"),
    };

    public WorkingHoursService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IEnumerable<WorkingHoursResponseDto>>> GetScheduleAsync()
    {
        try
        {
            var rows = await _context.WorkingHours.ToListAsync();

            // Return all 7 days; fill gaps with defaults when not yet saved
            var schedule = DayOrder.Select(day =>
            {
                var row = rows.FirstOrDefault(r => r.Day == day);
                if (row != null)
                    return new WorkingHoursResponseDto(row.Id, row.Day, row.OpenTime, row.CloseTime, row.IsClosed);

                var def = Defaults[day];
                return new WorkingHoursResponseDto(Guid.Empty, day, def.Open, def.Close, false);
            });

            return ApiResponse<IEnumerable<WorkingHoursResponseDto>>.Ok(schedule);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<WorkingHoursResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<WorkingHoursResponseDto>>> UpsertScheduleAsync(
        IEnumerable<UpsertWorkingHoursItemDto> items)
    {
        try
        {
            // Replace all existing rows for this tenant with the new schedule
            var existing = await _context.WorkingHours.ToListAsync();
            _context.WorkingHours.RemoveRange(existing);

            var newRows = items.Select(item => new WorkingHours
            {
                Day      = item.Day,
                OpenTime = item.OpenTime,
                CloseTime = item.CloseTime,
                IsClosed = item.IsClosed,
            }).ToList();

            await _context.WorkingHours.AddRangeAsync(newRows);
            await _context.SaveChangesAsync();

            return ApiResponse<IEnumerable<WorkingHoursResponseDto>>.Ok(
                newRows.Select(r => new WorkingHoursResponseDto(r.Id, r.Day, r.OpenTime, r.CloseTime, r.IsClosed)),
                "Schedule saved.");
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<WorkingHoursResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }
}
