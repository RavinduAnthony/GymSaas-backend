using GymSaaS.Application.DTOs.Memberships;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class MembershipService
{
    private readonly IRepository<Membership> _membershipRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IRepository<MembershipPackage> _packageRepo;
    private readonly PaymentService _paymentService;

    public MembershipService(
        IRepository<Membership> membershipRepo,
        IRepository<Payment> paymentRepo,
        IRepository<MembershipPackage> packageRepo,
        PaymentService paymentService)
    {
        _membershipRepo = membershipRepo;
        _paymentRepo = paymentRepo;
        _packageRepo = packageRepo;
        _paymentService = paymentService;
    }

    public async Task<ApiResponse<IEnumerable<MembershipResponseDto>>> GetByMemberAsync(Guid memberId)
    {
        try
        {
            var memberships = await _membershipRepo.FindAsync(m => m.MemberId == memberId);
            var packages = await _packageRepo.GetAllAsync();
            var packageDict = packages.ToDictionary(p => p.Id, p => p.Name);

            var dtos = memberships.Select(m =>
            {
                var dto = MapToDto(m);
                dto.PackageName = packageDict.TryGetValue(m.PackageId, out var name) ? name : string.Empty;
                return dto;
            });

            return ApiResponse<IEnumerable<MembershipResponseDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<MembershipResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MembershipResponseDto>> UpdateAsync(Guid membershipId, UpdateMembershipDto dto)
    {
        try
        {
            var membership = await _membershipRepo.GetByIdAsync(membershipId);
            if (membership == null) return ApiResponse<MembershipResponseDto>.Fail("Membership not found.");

            membership.PackageId = dto.PackageId;
            membership.StartDate = dto.StartDate;
            membership.EndDate = dto.EndDate;
            membership.Price = dto.Price;
            membership.Discount = dto.Discount;
            membership.PaymentStatus = dto.PaymentStatus;

            _membershipRepo.Update(membership);
            await _membershipRepo.SaveChangesAsync();

            var packages = await _packageRepo.GetAllAsync();
            var packageName = packages.FirstOrDefault(p => p.Id == membership.PackageId)?.Name ?? string.Empty;

            var responseDto = MapToDto(membership);
            responseDto.PackageName = packageName;
            return ApiResponse<MembershipResponseDto>.Ok(responseDto, "Membership updated.");
        }
        catch (Exception ex)
        {
            return ApiResponse<MembershipResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MembershipResponseDto>> CreateAsync(CreateMembershipDto dto)
    {
        try
        {
            var membership = new Membership
            {
                MemberId = dto.MemberId,
                PackageId = dto.PackageId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Price = dto.Price,
                Discount = dto.Discount,
                PaymentStatus = dto.PaymentStatus,
            };

            await _membershipRepo.AddAsync(membership);
            await _membershipRepo.SaveChangesAsync();

            var package = await _packageRepo.GetByIdAsync(dto.PackageId);

            // Inclusive month count: StartDate=Jan, EndDate=May → 5 months (Jan,Feb,Mar,Apr,May)
            var start = dto.StartDate;
            var end   = dto.EndDate;
            int durationMonths = ((end.Year - start.Year) * 12) + (end.Month - start.Month) + 1;
            if (durationMonths <= 0) durationMonths = 1;

            // Generate one PaymentSchedule row per billing month
            await _paymentService.GenerateScheduleAsync(
                membership.Id,
                dto.MemberId,
                durationMonths,
                monthlyAmount: dto.Price,
                registrationFee: 0,
                billingFrequency: package?.BillingFrequency ?? "Monthly",
                overrideFirstPayMonth: new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc));

            // If already marked Paid at creation, auto-mark initial schedules as paid
            if (dto.PaymentStatus == "Paid")
                await _paymentService.MarkInitialPaymentsAsPaidAsync(membership.Id, package?.Name ?? string.Empty, DateTime.UtcNow);

            var responseDto = MapToDto(membership);
            responseDto.PackageName = package?.Name ?? string.Empty;
            return ApiResponse<MembershipResponseDto>.Ok(responseDto, "Membership created.");
        }
        catch (Exception ex)
        {
            return ApiResponse<MembershipResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse> RenewAsync(RenewMembershipDto dto)
    {
        try
        {
            var membership = await _membershipRepo.GetByIdAsync(dto.MembershipId);
            if (membership == null) return ApiResponse.Fail("Membership not found.");

            // Capture the old end date before updating — new schedules start from the month after it
            var oldEndDate = membership.EndDate;

            membership.EndDate = dto.NewEndDate;
            membership.PaymentStatus = "Paid";
            _membershipRepo.Update(membership);

            // Record payment
            var payment = new Payment
            {
                MemberId = membership.MemberId,
                MembershipId = membership.Id,
                Amount = dto.Amount - dto.Discount,
                Date = DateTime.UtcNow,
                PlanName = dto.PlanName,
            };

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            // Generate payment schedules for the newly added months only
            // (from the month after the old end date, through the new end date)
            var newStart = new DateTime(oldEndDate.Year, oldEndDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            var newEnd   = dto.NewEndDate;
            int addedMonths = ((newEnd.Year - newStart.Year) * 12) + (newEnd.Month - newStart.Month) + 1;
            if (addedMonths > 0)
            {
                var package = await _packageRepo.GetByIdAsync(membership.PackageId);
                await _paymentService.GenerateScheduleAsync(
                    membership.Id,
                    membership.MemberId,
                    addedMonths,
                    monthlyAmount: dto.Amount - dto.Discount,
                    registrationFee: 0,
                    billingFrequency: package?.BillingFrequency ?? "Monthly",
                    overrideFirstPayMonth: newStart);
            }

            return ApiResponse.Ok("Membership renewed and payment recorded.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<PaymentResponseDto>>> GetPaymentsByMemberAsync(Guid memberId)
    {
        try
        {
            var payments = await _paymentRepo.FindAsync(p => p.MemberId == memberId);
            var dtos = payments.Select(p => new PaymentResponseDto
            {
                Id = p.Id,
                MemberId = p.MemberId,
                Amount = p.Amount,
                Date = p.Date,
                PlanName = p.PlanName,
            });
            return ApiResponse<IEnumerable<PaymentResponseDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<PaymentResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    private static MembershipResponseDto MapToDto(Membership m) => new()
    {
        Id = m.Id,
        MemberId = m.MemberId,
        PackageId = m.PackageId,
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        Price = m.Price,
        Discount = m.Discount,
        PaymentStatus = m.PaymentStatus,
        CreatedAt = m.CreatedAt,
    };
}
