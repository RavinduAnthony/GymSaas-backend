using GymSaaS.Application.DTOs.Members;
using GymSaaS.Application.Interfaces;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;

namespace GymSaaS.Application.Services;

public class MemberService : IMemberService
{
    private readonly IRepository<Member> _memberRepo;
    private readonly IRepository<MemberDeletionLog> _deletionLogRepo;

    public MemberService(IRepository<Member> memberRepo, IRepository<MemberDeletionLog> deletionLogRepo)
    {
        _memberRepo = memberRepo;
        _deletionLogRepo = deletionLogRepo;
    }

    public async Task<ApiResponse<IEnumerable<MemberResponseDto>>> GetAllAsync()
    {
        try
        {
            var members = await _memberRepo.FindAsync(m => m.Status != "Inactive");
            var dtos = members.Select(MapToDto);
            return ApiResponse<IEnumerable<MemberResponseDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<MemberResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<MemberResponseDto>>> GetInactiveAsync()
    {
        try
        {
            var members = await _memberRepo.FindAsync(m => m.Status == "Inactive");
            var dtos = members.Select(MapToDto);
            return ApiResponse<IEnumerable<MemberResponseDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<MemberResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MemberResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null) return ApiResponse<MemberResponseDto>.Fail("Member not found.");
            return ApiResponse<MemberResponseDto>.Ok(MapToDto(member));
        }
        catch (Exception ex)
        {
            return ApiResponse<MemberResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MemberResponseDto>> CreateAsync(CreateMemberDto dto)
    {
        try
        {
            var member = new Member
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                JoinDate = dto.JoinDate,
                BranchId = dto.BranchId,
                Email = dto.Email,
                EmergencyContact = dto.EmergencyContact,
                Address = dto.Address,
                Height = dto.Height,
                Weight = dto.Weight,
                MedicalConditions = dto.MedicalConditions,
                TrainerId = dto.TrainerId,
            };

            await _memberRepo.AddAsync(member);
            await _memberRepo.SaveChangesAsync();

            return ApiResponse<MemberResponseDto>.Ok(MapToDto(member), "Member created successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<MemberResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MemberResponseDto>> UpdateAsync(Guid id, UpdateMemberDto dto)
    {
        try
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null) return ApiResponse<MemberResponseDto>.Fail("Member not found.");

            member.FirstName = dto.FirstName;
            member.LastName = dto.LastName;
            member.Phone = dto.Phone;
            member.Gender = dto.Gender;
            member.DateOfBirth = dto.DateOfBirth;
            member.JoinDate = dto.JoinDate;
            member.BranchId = dto.BranchId;
            member.Status = dto.Status;
            member.Email = dto.Email;
            member.EmergencyContact = dto.EmergencyContact;
            member.Address = dto.Address;
            member.Height = dto.Height;
            member.Weight = dto.Weight;
            member.MedicalConditions = dto.MedicalConditions;
            member.TrainerId = dto.TrainerId;

            _memberRepo.Update(member);
            await _memberRepo.SaveChangesAsync();

            return ApiResponse<MemberResponseDto>.Ok(MapToDto(member), "Member updated successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<MemberResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        try
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null) return ApiResponse.Fail("Member not found.");

            // Snapshot member details before hard-deleting
            var log = new MemberDeletionLog
            {
                OriginalMemberId = member.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Phone = member.Phone,
                Email = member.Email,
                Gender = member.Gender,
                DateOfBirth = member.DateOfBirth,
                JoinDate = member.JoinDate,
                DeletedAt = DateTime.UtcNow,
            };
            await _deletionLogRepo.AddAsync(log);

            _memberRepo.Delete(member);
            await _memberRepo.SaveChangesAsync();

            return ApiResponse.Ok("Member permanently deleted.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeactivateAsync(Guid id)
    {
        try
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null) return ApiResponse.Fail("Member not found.");

            member.Status = "Inactive";
            _memberRepo.Update(member);
            await _memberRepo.SaveChangesAsync();

            return ApiResponse.Ok("Member deactivated successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse> ReactivateAsync(Guid id)
    {
        try
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null) return ApiResponse.Fail("Member not found.");
            if (member.Status != "Inactive") return ApiResponse.Fail("Member is not deactivated.");

            member.Status = "Active";
            _memberRepo.Update(member);
            await _memberRepo.SaveChangesAsync();

            return ApiResponse.Ok("Member reactivated successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"An error occurred: {ex.Message}");
        }
    }

    private static MemberResponseDto MapToDto(Member m) => new()
    {
        Id = m.Id,
        FirstName = m.FirstName,
        LastName = m.LastName,
        Phone = m.Phone,
        Gender = m.Gender,
        DateOfBirth = m.DateOfBirth,
        JoinDate = m.JoinDate,
        BranchId = m.BranchId,
        BranchName = m.Branch?.Name,
        Status = m.Status,
        MemberType = m.MemberType,
        Email = m.Email,
        EmergencyContact = m.EmergencyContact,
        Address = m.Address,
        Height = m.Height,
        Weight = m.Weight,
        MedicalConditions = m.MedicalConditions,
        TrainerId = m.TrainerId,
        CreatedAt = m.CreatedAt,
    };
}
