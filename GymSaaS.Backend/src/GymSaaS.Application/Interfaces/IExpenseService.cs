using GymSaaS.Application.DTOs.Expenses;
using GymSaaS.Shared;

namespace GymSaaS.Application.Interfaces;

public interface IExpenseService
{
    // Fixed
    Task<ApiResponse<IEnumerable<FixedExpenseResponseDto>>> GetFixedExpensesAsync();
    Task<ApiResponse<FixedExpenseResponseDto>> CreateFixedExpenseAsync(CreateFixedExpenseDto dto);
    Task<ApiResponse<FixedExpenseResponseDto>> UpdateFixedExpenseAsync(Guid id, UpdateFixedExpenseDto dto);
    Task<ApiResponse<string>> DeleteFixedExpenseAsync(Guid id);

    // Variable
    Task<ApiResponse<IEnumerable<VariableExpenseResponseDto>>> GetVariableExpensesAsync(int month, int year);
    Task<ApiResponse<VariableExpenseResponseDto>> CreateVariableExpenseAsync(CreateVariableExpenseDto dto);
    Task<ApiResponse<VariableExpenseResponseDto>> UpdateVariableExpenseAsync(Guid id, UpdateVariableExpenseDto dto);
    Task<ApiResponse<string>> DeleteVariableExpenseAsync(Guid id);
}
