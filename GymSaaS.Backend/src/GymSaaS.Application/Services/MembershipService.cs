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
    private readonly IRepository<Member> _memberRepo;
    private readonly PaymentService _paymentService;

    public MembershipService(
        IRepository<Membership> membershipRepo,
        IRepository<Payment> paymentRepo,
        IRepository<MembershipPackage> packageRepo,
        IRepository<Member> memberRepo,
        PaymentService paymentService)
    {
        _membershipRepo = membershipRepo;
        _paymentRepo = paymentRepo;
        _packageRepo = packageRepo;
        _memberRepo = memberRepo;
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
            var packageName = package?.Name ?? string.Empty;

            // Compute duration in months from the dates
            int durationMonths = ((dto.EndDate.Year - dto.StartDate.Year) * 12)
                                  + (dto.EndDate.Month - dto.StartDate.Month);
            if (durationMonths <= 0) durationMonths = 1;

            // Generate payment schedule (correct path chosen via billingFrequency)
            await _paymentService.GenerateScheduleAsync(
                membership.Id, dto.MemberId,
                durationMonths, dto.Price, dto.RegistrationFee,
                package?.BillingFrequency ?? "Monthly");

            // Auto-set MemberType on the member based on the package billing mode
            // "FullPayment" package => Type B (Special) | "Monthly" => Type A (Monthly)
            var member = await _memberRepo.GetByIdAsync(dto.MemberId);
            if (member != null)
            {
                member.MemberType = package?.BillingFrequency == "FullPayment"
                    ? AppConstants.MemberTypes.Special
                    : AppConstants.MemberTypes.Monthly;
                _memberRepo.Update(member);
                await _memberRepo.SaveChangesAsync();
            }

            // If paid at registration, immediately mark initial payments as Paid
            if (string.Equals(dto.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                await _paymentService.MarkInitialPaymentsAsPaidAsync(membership.Id, packageName, DateTime.UtcNow);
            }

            var responseDto = MapToDto(membership);
            responseDto.PackageName = packageName;
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
