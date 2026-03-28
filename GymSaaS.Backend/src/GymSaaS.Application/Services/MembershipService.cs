using GymSaaS.Application.DTOs.Memberships;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;

namespace GymSaaS.Application.Services;

public class MembershipService
{
    private readonly IRepository<Membership> _membershipRepo;
    private readonly IRepository<Payment> _paymentRepo;

    public MembershipService(IRepository<Membership> membershipRepo, IRepository<Payment> paymentRepo)
    {
        _membershipRepo = membershipRepo;
        _paymentRepo = paymentRepo;
    }

    public async Task<ApiResponse<IEnumerable<MembershipResponseDto>>> GetByMemberAsync(Guid memberId)
    {
        try
        {
            var memberships = await _membershipRepo.FindAsync(m => m.MemberId == memberId);
            return ApiResponse<IEnumerable<MembershipResponseDto>>.Ok(memberships.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<MembershipResponseDto>>.Fail($"An error occurred: {ex.Message}");
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

            return ApiResponse<MembershipResponseDto>.Ok(MapToDto(membership), "Membership created.");
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
