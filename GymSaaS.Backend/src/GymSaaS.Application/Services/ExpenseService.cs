using GymSaaS.Application.DTOs.Expenses;
using GymSaaS.Application.Interfaces;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Persistence;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly GymDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ExpenseService(GymDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    // ─── Fixed Expenses ───────────────────────────────────────────────────────

    public async Task<ApiResponse<IEnumerable<FixedExpenseResponseDto>>> GetFixedExpensesAsync()
    {
        try
        {
            var expenses = await _context.FixedExpenses
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            return ApiResponse<IEnumerable<FixedExpenseResponseDto>>.Ok(
                expenses.Select(MapFixed));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<FixedExpenseResponseDto>>.Fail($"Failed to retrieve fixed expenses: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FixedExpenseResponseDto>> CreateFixedExpenseAsync(CreateFixedExpenseDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApiResponse<FixedExpenseResponseDto>.Fail("Expense name is required.");

            if (dto.Amount < 0)
                return ApiResponse<FixedExpenseResponseDto>.Fail("Amount cannot be negative.");

            var expense = new FixedExpense
            {
                TenantId = _tenantProvider.TenantId,
                Name = dto.Name.Trim(),
                Amount = dto.Amount,
                Description = dto.Description?.Trim(),
                IsActive = true,
            };

            await _context.FixedExpenses.AddAsync(expense);
            await _context.SaveChangesAsync();

            return ApiResponse<FixedExpenseResponseDto>.Ok(MapFixed(expense));
        }
        catch (Exception ex)
        {
            return ApiResponse<FixedExpenseResponseDto>.Fail($"Failed to create fixed expense: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FixedExpenseResponseDto>> UpdateFixedExpenseAsync(Guid id, UpdateFixedExpenseDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApiResponse<FixedExpenseResponseDto>.Fail("Expense name is required.");

            if (dto.Amount < 0)
                return ApiResponse<FixedExpenseResponseDto>.Fail("Amount cannot be negative.");

            var expense = await _context.FixedExpenses.FindAsync(id);
            if (expense == null)
                return ApiResponse<FixedExpenseResponseDto>.Fail("Fixed expense not found.");

            expense.Name = dto.Name.Trim();
            expense.Amount = dto.Amount;
            expense.Description = dto.Description?.Trim();
            expense.IsActive = dto.IsActive;
            expense.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<FixedExpenseResponseDto>.Ok(MapFixed(expense));
        }
        catch (Exception ex)
        {
            return ApiResponse<FixedExpenseResponseDto>.Fail($"Failed to update fixed expense: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteFixedExpenseAsync(Guid id)
    {
        try
        {
            var expense = await _context.FixedExpenses.FindAsync(id);
            if (expense == null)
                return ApiResponse<string>.Fail("Fixed expense not found.");

            _context.FixedExpenses.Remove(expense);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Fixed expense deleted.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"Failed to delete fixed expense: {ex.Message}");
        }
    }

    // ─── Variable Expenses ────────────────────────────────────────────────────

    public async Task<ApiResponse<IEnumerable<VariableExpenseResponseDto>>> GetVariableExpensesAsync(int month, int year)
    {
        try
        {
            var expenses = await _context.VariableExpenses
                .Where(e => e.Month == month && e.Year == year)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            return ApiResponse<IEnumerable<VariableExpenseResponseDto>>.Ok(
                expenses.Select(MapVariable));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<VariableExpenseResponseDto>>.Fail($"Failed to retrieve variable expenses: {ex.Message}");
        }
    }

    public async Task<ApiResponse<VariableExpenseResponseDto>> CreateVariableExpenseAsync(CreateVariableExpenseDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApiResponse<VariableExpenseResponseDto>.Fail("Expense name is required.");

            if (dto.Amount < 0)
                return ApiResponse<VariableExpenseResponseDto>.Fail("Amount cannot be negative.");

            if (dto.Month < 1 || dto.Month > 12)
                return ApiResponse<VariableExpenseResponseDto>.Fail("Month must be between 1 and 12.");

            if (dto.Year < 2000)
                return ApiResponse<VariableExpenseResponseDto>.Fail("Year must be 2000 or later.");

            var expense = new VariableExpense
            {
                TenantId = _tenantProvider.TenantId,
                Name = dto.Name.Trim(),
                Amount = dto.Amount,
                Description = dto.Description?.Trim(),
                Month = dto.Month,
                Year = dto.Year,
            };

            await _context.VariableExpenses.AddAsync(expense);
            await _context.SaveChangesAsync();

            return ApiResponse<VariableExpenseResponseDto>.Ok(MapVariable(expense));
        }
        catch (Exception ex)
        {
            return ApiResponse<VariableExpenseResponseDto>.Fail($"Failed to create variable expense: {ex.Message}");
        }
    }

    public async Task<ApiResponse<VariableExpenseResponseDto>> UpdateVariableExpenseAsync(Guid id, UpdateVariableExpenseDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApiResponse<VariableExpenseResponseDto>.Fail("Expense name is required.");

            if (dto.Amount < 0)
                return ApiResponse<VariableExpenseResponseDto>.Fail("Amount cannot be negative.");

            var expense = await _context.VariableExpenses.FindAsync(id);
            if (expense == null)
                return ApiResponse<VariableExpenseResponseDto>.Fail("Variable expense not found.");

            expense.Name = dto.Name.Trim();
            expense.Amount = dto.Amount;
            expense.Description = dto.Description?.Trim();
            expense.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<VariableExpenseResponseDto>.Ok(MapVariable(expense));
        }
        catch (Exception ex)
        {
            return ApiResponse<VariableExpenseResponseDto>.Fail($"Failed to update variable expense: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteVariableExpenseAsync(Guid id)
    {
        try
        {
            var expense = await _context.VariableExpenses.FindAsync(id);
            if (expense == null)
                return ApiResponse<string>.Fail("Variable expense not found.");

            _context.VariableExpenses.Remove(expense);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Variable expense deleted.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"Failed to delete variable expense: {ex.Message}");
        }
    }

    // ─── Mappers ──────────────────────────────────────────────────────────────

    private static FixedExpenseResponseDto MapFixed(FixedExpense e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Amount = e.Amount,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    private static VariableExpenseResponseDto MapVariable(VariableExpense e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Amount = e.Amount,
        Description = e.Description,
        Month = e.Month,
        Year = e.Year,
        CreatedAt = e.CreatedAt,
    };
}
