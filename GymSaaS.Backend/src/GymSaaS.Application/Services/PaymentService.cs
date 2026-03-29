using GymSaaS.Application.DTOs.Payments;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;

namespace GymSaaS.Application.Services;

public class PaymentService
{
    private readonly IRepository<PaymentSchedule> _scheduleRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IRepository<Member> _memberRepo;
    private readonly IRepository<Membership> _membershipRepo;
    private readonly IRepository<MembershipPackage> _packageRepo;
    private readonly IRepository<PaymentType> _paymentTypeRepo;

    public PaymentService(
        IRepository<PaymentSchedule> scheduleRepo,
        IRepository<Payment> paymentRepo,
        IRepository<Member> memberRepo,
        IRepository<Membership> membershipRepo,
        IRepository<MembershipPackage> packageRepo,
        IRepository<PaymentType> paymentTypeRepo)
    {
        _scheduleRepo = scheduleRepo;
        _paymentRepo = paymentRepo;
        _memberRepo = memberRepo;
        _membershipRepo = membershipRepo;
        _packageRepo = packageRepo;
        _paymentTypeRepo = paymentTypeRepo;
    }

    // -------------------------------------------------------------------------
    // Schedule Generation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Generates PaymentSchedule records for a newly created membership.
    /// Business rules:
    ///  - Registration fee: due today (one-time)
    ///  - If today is in the LAST WEEK of the month ? first monthly payment shifts to next month
    ///  - Monthly payments due on 5th of each month
    /// </summary>
    public async Task GenerateScheduleAsync(
        Guid membershipId, Guid memberId,
        int durationMonths, decimal monthlyAmount, decimal registrationFee,
        string billingFrequency = "Monthly")
    {
        var today = DateTime.UtcNow;
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        bool isLastWeek = today.Day > daysInMonth - 7;

        // Registration fee (one-time, due today) - always applies regardless of billing mode
        if (registrationFee > 0)
        {
            await _scheduleRepo.AddAsync(new PaymentSchedule
            {
                MembershipId = membershipId,
                MemberId = memberId,
                DueDate = today.Date,
                Amount = registrationFee,
                Status = "Pending",
                PaymentTypeId = AppConstants.PaymentTypeIds.RegistrationFee,
                Month = today.ToString("yyyy-MM"),
            });
        }

        if (billingFrequency == "FullPayment")
        {
            // Single upfront payment for the entire duration
            var totalAmount = monthlyAmount * durationMonths;
            await _scheduleRepo.AddAsync(new PaymentSchedule
            {
                MembershipId = membershipId,
                MemberId = memberId,
                DueDate = today.Date,
                Amount = totalAmount,
                Status = "Pending",
                PaymentTypeId = AppConstants.PaymentTypeIds.MonthlyInitial,
                Month = today.ToString("yyyy-MM"),
            });
        }
        else
        {
            // Monthly: one schedule row per month, due on the 5th
            var firstPayMonth = isLastWeek
                ? new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)
                : new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            for (int i = 0; i < durationMonths; i++)
            {
                var payMonth = firstPayMonth.AddMonths(i);
                var dueDate = new DateTime(payMonth.Year, payMonth.Month, 5, 0, 0, 0, DateTimeKind.Utc);
                await _scheduleRepo.AddAsync(new PaymentSchedule
                {
                    MembershipId = membershipId,
                    MemberId = memberId,
                    DueDate = dueDate,
                    Amount = monthlyAmount,
                    Status = "Pending",
                    PaymentTypeId = i == 0
                        ? AppConstants.PaymentTypeIds.MonthlyInitial
                        : AppConstants.PaymentTypeIds.RegularMonthly,
                    Month = payMonth.ToString("yyyy-MM"),
                });
            }
        }

        await _scheduleRepo.SaveChangesAsync();
    }

    /// <summary>
    /// Marks RegistrationFee + MonthlyInitial schedules as Paid and records Payment entries.
    /// Called when paymentStatus == "Paid" at membership creation.
    /// </summary>
    public async Task MarkInitialPaymentsAsPaidAsync(Guid membershipId, string packageName, DateTime paidDate)
    {
        var schedules = await _scheduleRepo.FindAsync(
            s => s.MembershipId == membershipId &&
                 (s.PaymentTypeId == AppConstants.PaymentTypeIds.RegistrationFee ||
                  s.PaymentTypeId == AppConstants.PaymentTypeIds.MonthlyInitial));

        var dueMonth7th = new DateTime(paidDate.Year, paidDate.Month, 7, 23, 59, 59, DateTimeKind.Utc);
        bool isLate = paidDate > dueMonth7th;
        var status = isLate ? "Late" : "Paid";

        foreach (var s in schedules)
        {
            s.Status = status;
            s.PaidDate = paidDate;
            _scheduleRepo.Update(s);

            var memberId = s.MemberId;
            await _paymentRepo.AddAsync(new Payment
            {
                MemberId = memberId,
                MembershipId = membershipId,
                PaymentScheduleId = s.Id,
                Amount = s.Amount,
                Date = paidDate,
                PlanName = packageName,
                PaymentTypeId = s.PaymentTypeId,
                Status = status,
                Method = "Cash",
            });
        }

        await _scheduleRepo.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // Query
    // -------------------------------------------------------------------------

    public async Task<ApiResponse<IEnumerable<PaymentScheduleDto>>> GetAllSchedulesAsync()
    {
        try
        {
            await RefreshLateStatusInternalAsync();
            var schedules = await _scheduleRepo.GetAllAsync();
            var lookup = await BuildLookupAsync();
            var dtos = schedules.OrderBy(s => s.DueDate).Select(s => MapToDto(s, lookup));
            return ApiResponse<IEnumerable<PaymentScheduleDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<PaymentScheduleDto>>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<PaymentScheduleDto>>> GetSchedulesByMemberAsync(Guid memberId)
    {
        try
        {
            await RefreshLateStatusInternalAsync();
            var schedules = await _scheduleRepo.FindAsync(s => s.MemberId == memberId);
            var lookup = await BuildLookupAsync();
            var dtos = schedules.OrderBy(s => s.DueDate).Select(s => MapToDto(s, lookup));
            return ApiResponse<IEnumerable<PaymentScheduleDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<PaymentScheduleDto>>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<PaymentHistoryDto>>> GetPaymentHistoryAsync(Guid? memberId = null)
    {
        try
        {
            var payments = memberId.HasValue
                ? await _paymentRepo.FindAsync(p => p.MemberId == memberId.Value)
                : await _paymentRepo.GetAllAsync();

            var members = await _memberRepo.GetAllAsync();
            var memberships = await _membershipRepo.GetAllAsync();
            var packages = await _packageRepo.GetAllAsync();

            var memberDict = members.ToDictionary(m => m.Id, m => $"{m.FirstName} {m.LastName}");
            var packageDict = packages.ToDictionary(p => p.Id, p => p.Name);
            var msPackageMap = memberships.ToDictionary(
                m => m.Id,
                m => packageDict.TryGetValue(m.PackageId, out var n) ? n : "");

            var typeDict = (await _paymentTypeRepo.GetAllAsync())
                .ToDictionary(t => t.Id, t => t.Name);

            var dtos = payments.OrderByDescending(p => p.Date).Select(p => new PaymentHistoryDto
            {
                Id = p.Id,
                MemberId = p.MemberId,
                MemberName = memberDict.TryGetValue(p.MemberId, out var name) ? name : "Unknown",
                Date = p.Date,
                Amount = p.Amount,
                PaymentTypeId = p.PaymentTypeId,
                PaymentTypeName = typeDict.TryGetValue(p.PaymentTypeId, out var tn) ? tn : "",
                Status = p.Status,
                Method = p.Method,
                PlanName = !string.IsNullOrEmpty(p.PlanName) ? p.PlanName
                    : (p.MembershipId.HasValue && msPackageMap.TryGetValue(p.MembershipId.Value, out var pn) ? pn : ""),
                Notes = p.Notes,
            });

            return ApiResponse<IEnumerable<PaymentHistoryDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<PaymentHistoryDto>>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PaymentDashboardSummaryDto>> GetDashboardSummaryAsync()
    {
        try
        {
            await RefreshLateStatusInternalAsync();
            var schedules = await _scheduleRepo.GetAllAsync();
            var payments = await _paymentRepo.GetAllAsync();

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            var totalRevenue = payments.Sum(p => p.Amount);
            var thisMonthRevenue = payments
                .Where(p => p.Date >= monthStart && p.Date < monthEnd)
                .Sum(p => p.Amount);

            var pending = schedules.Where(s => s.Status == "Pending").ToList();
            var late = schedules.Where(s => s.Status == "Late").ToList();
            var paidThisMonth = schedules.Count(s =>
                s.PaidDate >= monthStart && s.PaidDate < monthEnd && s.Status == "Paid");

            return ApiResponse<PaymentDashboardSummaryDto>.Ok(new PaymentDashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                ThisMonthRevenue = thisMonthRevenue,
                PendingCount = pending.Count,
                PendingAmount = pending.Sum(s => s.Amount),
                LateCount = late.Count,
                LateAmount = late.Sum(s => s.Amount),
                PaidThisMonth = paidThisMonth,
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PaymentDashboardSummaryDto>.Fail($"Error: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // Record Payment
    // -------------------------------------------------------------------------

    public async Task<ApiResponse<PaymentScheduleDto>> RecordPaymentAsync(RecordPaymentDto dto)
    {
        try
        {
            var schedule = await _scheduleRepo.GetByIdAsync(dto.ScheduleId);
            if (schedule == null)
                return ApiResponse<PaymentScheduleDto>.Fail("Payment schedule not found.");
            if (schedule.Status == "Paid")
                return ApiResponse<PaymentScheduleDto>.Fail("This payment is already recorded.");

            var paidDate = (dto.PaidDate?.ToUniversalTime() ?? DateTime.UtcNow);

            // Late rule: paid after the 7th of the schedule's due month
            var cutoff = new DateTime(schedule.DueDate.Year, schedule.DueDate.Month, 7, 23, 59, 59, DateTimeKind.Utc);
            bool isLate = paidDate > cutoff;
            var status = isLate ? "Late" : "Paid";

            schedule.Status = status;
            schedule.PaidDate = paidDate;
            if (!string.IsNullOrWhiteSpace(dto.Notes)) schedule.Notes = dto.Notes;
            _scheduleRepo.Update(schedule);

            // Look up plan name
            var membership = await _membershipRepo.GetByIdAsync(schedule.MembershipId);
            var packageName = string.Empty;
            if (membership != null)
            {
                var pkg = await _packageRepo.GetByIdAsync(membership.PackageId);
                packageName = pkg?.Name ?? string.Empty;
            }

            await _paymentRepo.AddAsync(new Payment
            {
                MemberId = schedule.MemberId,
                MembershipId = schedule.MembershipId,
                PaymentScheduleId = schedule.Id,
                Amount = dto.Amount > 0 ? dto.Amount : schedule.Amount,
                Date = paidDate,
                PlanName = packageName,
                PaymentTypeId = schedule.PaymentTypeId,
                Status = status,
                Method = string.IsNullOrEmpty(dto.Method) ? "Cash" : dto.Method,
                Notes = dto.Notes,
            });

            await _scheduleRepo.SaveChangesAsync();

            var lookup = await BuildLookupAsync();
            return ApiResponse<PaymentScheduleDto>.Ok(
                MapToDto(schedule, lookup),
                isLate ? "Payment recorded — marked as LATE (after 7th)." : "Payment recorded successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<PaymentScheduleDto>.Fail($"Error: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    public async Task<ApiResponse<IEnumerable<PaymentTypeDto>>> GetPaymentTypesAsync()
    {
        try
        {
            var types = await _paymentTypeRepo.GetAllAsync();
            var dtos = types.Where(t => t.IsActive).OrderBy(t => t.Id)
                .Select(t => new PaymentTypeDto { Id = t.Id, Name = t.Name, Description = t.Description, IsActive = t.IsActive });
            return ApiResponse<IEnumerable<PaymentTypeDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<PaymentTypeDto>>.Fail($"Error: {ex.Message}");
        }
    }

    // Late Status Refresh
    // -------------------------------------------------------------------------

    public async Task<ApiResponse> RefreshLateStatusAsync()
    {
        try
        {
            await RefreshLateStatusInternalAsync();
            return ApiResponse.Ok("Late statuses refreshed.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error: {ex.Message}");
        }
    }

    private async Task RefreshLateStatusInternalAsync()
    {
        var now = DateTime.UtcNow;
        var pending = await _scheduleRepo.FindAsync(s => s.Status == "Pending");
        bool changed = false;
        foreach (var s in pending)
        {
            // Past the 7th of the due month and the due month/year has actually arrived
            var cutoff = new DateTime(s.DueDate.Year, s.DueDate.Month, 7, 23, 59, 59, DateTimeKind.Utc);
            if (now > cutoff)
            {
                s.Status = "Late";
                _scheduleRepo.Update(s);
                changed = true;
            }
        }
        if (changed) await _scheduleRepo.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<(Dictionary<Guid, string> Members, Dictionary<Guid, string> Packages, Dictionary<int, string> Types)> BuildLookupAsync()
    {
        var members = await _memberRepo.GetAllAsync();
        var memberships = await _membershipRepo.GetAllAsync();
        var packages = await _packageRepo.GetAllAsync();
        var types = await _paymentTypeRepo.GetAllAsync();

        var memberDict = members.ToDictionary(m => m.Id, m => $"{m.FirstName} {m.LastName}");
        var packageDict = packages.ToDictionary(p => p.Id, p => p.Name);
        var msPackageMap = memberships.ToDictionary(
            m => m.Id,
            m => packageDict.TryGetValue(m.PackageId, out var n) ? n : "");
        var typeDict = types.ToDictionary(t => t.Id, t => t.Name);

        return (memberDict, msPackageMap, typeDict);
    }

    private static PaymentScheduleDto MapToDto(
        PaymentSchedule s,
        (Dictionary<Guid, string> Members, Dictionary<Guid, string> Packages, Dictionary<int, string> Types) lookup)
        => new()
        {
            Id = s.Id,
            MemberId = s.MemberId,
            MemberName = lookup.Members.TryGetValue(s.MemberId, out var mn) ? mn : "Unknown",
            MembershipId = s.MembershipId,
            PackageName = lookup.Packages.TryGetValue(s.MembershipId, out var pn) ? pn : "",
            DueDate = s.DueDate,
            Amount = s.Amount,
            Status = s.Status,
            PaymentTypeId = s.PaymentTypeId,
            PaymentTypeName = lookup.Types.TryGetValue(s.PaymentTypeId, out var tn) ? tn : "",
            PaidDate = s.PaidDate,
            Notes = s.Notes,
            Month = s.Month,
        };
}
