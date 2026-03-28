using GymSaaS.Application.DTOs.Auth;
using GymSaaS.Shared;

namespace GymSaaS.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterTenantAsync(RegisterTenantDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
}
