using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Expenses;
using GymSaaS.Application.Interfaces;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    // ─── Fixed Expenses ──────────────────────────────────────────────────────

    [HttpGet("fixed")]
    public async Task<IActionResult> GetFixedExpenses()
    {
        try
        {
            var result = await _expenseService.GetFixedExpensesAsync();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
        }
    }

    [HttpPost("fixed")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> CreateFixedExpense([FromBody] CreateFixedExpenseDto dto)
    {
        try
        {
            var result = await _expenseService.CreateFixedExpenseAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
        }
    }

    [HttpPut("fixed/{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> UpdateFixedExpense(Guid id, [FromBody] UpdateFixedExpenseDto dto)
    {
        try
        {
            var result = await _expenseService.UpdateFixedExpenseAsync(id, dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
        }
    }

    [HttpDelete("fixed/{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> DeleteFixedExpense(Guid id)
    {
        try
        {
            var result = await _expenseService.DeleteFixedExpenseAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
        }
    }

    // ─── Variable Expenses ───────────────────────────────────────────────────

    [HttpGet("variable")]
    public async Task<IActionResult> GetVariableExpenses([FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            var result = await _expenseService.GetVariableExpensesAsync(month, year);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
        }
    }

    [HttpPost("variable")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> CreateVariableExpense([FromBody] CreateVariableExpenseDto dto)
    {
        try
        {
            var result = await _expenseService.CreateVariableExpenseAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
        }
    }

    [HttpPut("variable/{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> UpdateVariableExpense(Guid id, [FromBody] UpdateVariableExpenseDto dto)
    {
        try
        {
            var result = await _expenseService.UpdateVariableExpenseAsync(id, dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
        }
    }

    [HttpDelete("variable/{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> DeleteVariableExpense(Guid id)
    {
        try
        {
            var result = await _expenseService.DeleteVariableExpenseAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
        }
    }
}
