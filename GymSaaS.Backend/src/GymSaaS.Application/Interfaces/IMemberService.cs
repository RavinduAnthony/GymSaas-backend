using GymSaaS.Application.DTOs.Members;
using GymSaaS.Shared;

namespace GymSaaS.Application.Interfaces;

public interface IMemberService
{
    Task<ApiResponse<IEnumerable<MemberResponseDto>>> GetAllAsync();
    Task<ApiResponse<MemberResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<MemberResponseDto>> CreateAsync(CreateMemberDto dto);
    Task<ApiResponse<MemberResponseDto>> UpdateAsync(Guid id, UpdateMemberDto dto);
    Task<ApiResponse> DeleteAsync(Guid id);
}
