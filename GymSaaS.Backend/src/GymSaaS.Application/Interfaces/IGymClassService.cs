using GymSaaS.Application.DTOs.Classes;
using GymSaaS.Shared;

namespace GymSaaS.Application.Interfaces;

public interface IGymClassService
{
    Task<ApiResponse<IEnumerable<GymClassResponseDto>>> GetAllAsync();
    Task<ApiResponse<GymClassResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<GymClassResponseDto>> CreateAsync(CreateGymClassDto dto);
    Task<ApiResponse<GymClassResponseDto>> UpdateAsync(Guid id, UpdateGymClassDto dto);
    Task<ApiResponse> DeleteAsync(Guid id);
}
