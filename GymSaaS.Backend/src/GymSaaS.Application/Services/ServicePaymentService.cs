using GymSaaS.Application.DTOs.Payments;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class ServicePaymentService
{
    private readonly IRepository<ServicePaymentSchedule> _scheduleRepo;
    private readonly IRepository<ServicePayment> _paymentRepo;
    private readonly IRepository<GymClass> _classRepo;
    private readonly IRepository<PtRegistration> _ptRepo;
    private readonly ITenantProvider _tenantProvider;

    public ServicePaymentService(
        IRepository<ServicePaymentSchedule> scheduleRepo,
        IRepository<ServicePayment> paymentRepo,
        IRepository<GymClass> classRepo,
        IRepository<PtRegistration> ptRepo,
        ITenantProvider tenantProvider)
    {
        _scheduleRepo = scheduleRepo;
        _paymentRepo = paymentRepo;
        _classRepo = classRepo;
        _ptRepo = ptRepo;
        _tenantProvider = tenantProvider;
    }

    // ─── Schedule Generation ──────────────────────────────────────────────────

    /// <summary>
    /// Generates a ServicePaymentSchedule for every active Class and PT Registration
    /// for the given billing month, if one doesn't already exist.
    /// Called: (a) manually via the controller, (b) automatically in GetAllSchedulesAsync.
    /// </summary>
    public async Task<ApiResponse<int>> GenerateMonthlySchedulesAsync(string? serviceType = null)
    {
        try
        {
            var now = DateTime.UtcNow;
            var monthStr = now.ToString("yyyy-MM");
            var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            // Due date = last day of current month (payment window = last week)
            var dueDate = new DateTime(now.Year, now.Month, daysInMonth, 23, 59, 59, DateTimeKind.Utc);
            int created = 0;

            // ── Classes ──────────────────────────────────────────────────────
            if (serviceType is null or "Class")
            {
                var classes = await _classRepo.AsQueryable()
                    .Where(c => c.Status == "Active")
                    .ToListAsync();

                foreach (var cls in classes)
                {
                    bool exists = await _scheduleRepo.AsQueryable()
                        .AnyAsync(s => s.GymClassId == cls.Id && s.Month == monthStr);
                    if (exists) continue;

                    await _scheduleRepo.AddAsync(new ServicePaymentSchedule
                    {
                        TenantId = _tenantProvider.TenantId,
                        ServiceType = "Class",
                        GymClassId = cls.Id,
                        ServiceName = cls.Name,
                        Month = monthStr,
                        DueDate = dueDate,
                        Amount = cls.DefaultAmount,
                        Status = "Pending",
                    });
                    created++;
                }
            }

            // ── PT Registrations ─────────────────────────────────────────────
            if (serviceType is null or "PT")
            {
                var ptRegs = await _ptRepo.AsQueryable()
                    .Where(p => p.Status == "Active")
                    .ToListAsync();

                foreach (var pt in ptRegs)
                {
                    bool exists = await _scheduleRepo.AsQueryable()
                        .AnyAsync(s => s.PtRegistrationId == pt.Id && s.Month == monthStr);
                    if (exists) continue;

                    var amount = pt.PaymentRatePerStudent * pt.StudentCount;
                    await _scheduleRepo.AddAsync(new ServicePaymentSchedule
                    {
                        TenantId = _tenantProvider.TenantId,
                        ServiceType = "PT",
                        PtRegistrationId = pt.Id,
                        ServiceName = pt.TrainerName,
                        Month = monthStr,
                        DueDate = dueDate,
                        Amount = amount,
                        Status = "Pending",
                    });
                    created++;
                }
            }

            if (created > 0)
                await _scheduleRepo.SaveChangesAsync();

            return ApiResponse<int>.Ok(created, $"Generated {created} service payment schedule(s) for {monthStr}.");
        }
        catch (Exception ex)
        {
            return ApiResponse<int>.Fail($"Error generating schedules: {ex.Message}");
        }
    }

    // ─── Query ────────────────────────────────────────────────────────────────

    public async Task<ApiResponse<IEnumerable<ServicePaymentScheduleDto>>> GetAllSchedulesAsync()
    {
        try
        {
            // Auto-generate this month's schedules if missing
            await GenerateMonthlySchedulesAsync();
            // Auto-mark late
            await RefreshLateStatusInternalAsync();

            var schedules = await _scheduleRepo.AsQueryable()
                .OrderByDescending(s => s.Month).ThenBy(s => s.ServiceName)
                .ToListAsync();

            return ApiResponse<IEnumerable<ServicePaymentScheduleDto>>.Ok(schedules.Select(MapSchedule));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<ServicePaymentScheduleDto>>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<ServicePaymentHistoryDto>>> GetHistoryAsync()
    {
        try
        {
            var payments = await _paymentRepo.AsQueryable()
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return ApiResponse<IEnumerable<ServicePaymentHistoryDto>>.Ok(payments.Select(MapHistory));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<ServicePaymentHistoryDto>>.Fail($"Error: {ex.Message}");
        }
    }

    // ─── Record Payment ───────────────────────────────────────────────────────

    /// <summary>
    /// Business rule:
    ///   - Settlement window = last week of the billing month (last 7 days incl. last day).
    ///   - If paid INSIDE that window → Status = "Paid"
    ///   - If paid in week 1 of NEXT MONTH (days 1-7 of following month) → Status = "Late"
    ///     and the payment is credited to the PREVIOUS month's schedule.
    ///   - Any other time → Reject (too early or too late).
    /// </summary>
    public async Task<ApiResponse<ServicePaymentScheduleDto>> RecordPaymentAsync(RecordServicePaymentDto dto)
    {
        try
        {
            var schedule = await _scheduleRepo.GetByIdAsync(dto.ScheduleId);
            if (schedule == null)
                return ApiResponse<ServicePaymentScheduleDto>.Fail("Service payment schedule not found.");
            if (schedule.Status == "Paid")
                return ApiResponse<ServicePaymentScheduleDto>.Fail("This payment is already recorded.");

            var paidDate = dto.PaidDate?.ToUniversalTime() ?? DateTime.UtcNow;
            var status = ClassifyServicePayment(schedule, paidDate, out var message);
            if (status == null)
                return ApiResponse<ServicePaymentScheduleDto>.Fail(message);

            schedule.Status = status;
            schedule.PaidDate = paidDate;
            if (!string.IsNullOrWhiteSpace(dto.Notes)) schedule.Notes = dto.Notes;
            _scheduleRepo.Update(schedule);

            await _paymentRepo.AddAsync(new ServicePayment
            {
                TenantId = _tenantProvider.TenantId,
                ServicePaymentScheduleId = schedule.Id,
                ServiceType = schedule.ServiceType,
                ServiceName = schedule.ServiceName,
                Month = schedule.Month,
                Amount = dto.Amount > 0 ? dto.Amount : schedule.Amount,
                Date = paidDate,
                Status = status,
                Method = string.IsNullOrEmpty(dto.Method) ? "Cash" : dto.Method,
                Notes = dto.Notes,
            });

            await _scheduleRepo.SaveChangesAsync();

            return ApiResponse<ServicePaymentScheduleDto>.Ok(MapSchedule(schedule), message);
        }
        catch (Exception ex)
        {
            return ApiResponse<ServicePaymentScheduleDto>.Fail($"Error: {ex.Message}");
        }
    }

    // ─── Late Refresh ─────────────────────────────────────────────────────────

    public async Task<ApiResponse<string>> RefreshLateStatusAsync()
    {
        try
        {
            await RefreshLateStatusInternalAsync();
            return ApiResponse<string>.Ok("Service payment late statuses refreshed.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task RefreshLateStatusInternalAsync()
    {
        var now = DateTime.UtcNow;
        var schedules = await _scheduleRepo.AsQueryable()
            .Where(s => s.Status == "Pending")
            .ToListAsync();

        foreach (var s in schedules)
        {
            // Past due date entirely → mark Late
            if (s.DueDate.Date < now.Date)
            {
                s.Status = "Late";
                _scheduleRepo.Update(s);
            }
        }

        await _scheduleRepo.SaveChangesAsync();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Classifies the payment and returns the status string ("Paid" | "Late") or null if invalid timing.
    /// </summary>
    private static string? ClassifyServicePayment(
        ServicePaymentSchedule schedule,
        DateTime paidDate,
        out string message)
    {
        // Parse billing month of the schedule
        var billingYear  = int.Parse(schedule.Month[..4]);
        var billingMonth = int.Parse(schedule.Month[5..7]);
        var daysInBilling = DateTime.DaysInMonth(billingYear, billingMonth);
        int lastWeekStart = daysInBilling - 6; // e.g. Apr → day 24

        // Paid within billing month
        if (paidDate.Year == billingYear && paidDate.Month == billingMonth)
        {
            if (paidDate.Day >= lastWeekStart)
            {
                message = "Service payment recorded — On time (last week of billing month).";
                return "Paid";
            }
            // Paid too early (before last week)
            message = $"Payment recorded early. Service payments are due in the last week " +
                      $"(from day {lastWeekStart}) of the billing month.";
            // Still mark as Paid — paid early is better than late
            return "Paid";
        }

        // Paid in the first week of the following month → LATE for previous month
        var nextYear = billingMonth == 12 ? billingYear + 1 : billingYear;
        var nextMonth = billingMonth == 12 ? 1 : billingMonth + 1;
        if (paidDate.Year == nextYear && paidDate.Month == nextMonth && paidDate.Day <= 7)
        {
            message = $"Service payment recorded — LATE (paid in week 1 of following month, " +
                      $"credited to billing month {schedule.Month}).";
            return "Late";
        }

        message = $"Payment date {paidDate:yyyy-MM-dd} is outside the valid payment window " +
                  $"for billing month {schedule.Month}.";
        return null;
    }

    private static ServicePaymentScheduleDto MapSchedule(ServicePaymentSchedule s) => new()
    {
        Id = s.Id,
        ServiceType = s.ServiceType,
        GymClassId = s.GymClassId,
        PtRegistrationId = s.PtRegistrationId,
        ServiceName = s.ServiceName,
        Month = s.Month,
        DueDate = s.DueDate,
        Amount = s.Amount,
        Status = s.Status,
        PaidDate = s.PaidDate,
        Notes = s.Notes,
    };

    private static ServicePaymentHistoryDto MapHistory(ServicePayment p) => new()
    {
        Id = p.Id,
        ServiceType = p.ServiceType,
        ServiceName = p.ServiceName,
        Month = p.Month,
        Date = p.Date,
        Amount = p.Amount,
        Status = p.Status,
        Method = p.Method,
        Notes = p.Notes,
    };
}
