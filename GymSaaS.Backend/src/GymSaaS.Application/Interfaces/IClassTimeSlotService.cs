using GymSaaS.Application.DTOs.Classes;
using GymSaaS.Shared;

namespace GymSaaS.Application.Interfaces;

public interface IClassTimeSlotService
{
    Task<ApiResponse<IEnumerable<ClassTimeSlotResponseDto>>> GetByBranchAsync(Guid branchId, Guid? excludeClassId = null);
}
