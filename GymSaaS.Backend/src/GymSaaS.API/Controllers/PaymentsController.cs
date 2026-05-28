using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Payments;
using GymSaaS.Application.Services;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// GET api/payments/schedules — all payment schedules (with late refresh)
    [HttpGet("schedules")]
    public async Task<IActionResult> GetAllSchedules()
        => Ok(await _paymentService.GetAllSchedulesAsync());

    /// GET api/payments/types — master list of payment types (for extensibility)
    [HttpGet("types")]
    public async Task<IActionResult> GetPaymentTypes()
        => Ok(await _paymentService.GetPaymentTypesAsync());

    /// GET api/payments/schedules/member/{memberId}
    [HttpGet("schedules/member/{memberId:guid}")]
    public async Task<IActionResult> GetSchedulesByMember(Guid memberId)
        => Ok(await _paymentService.GetSchedulesByMemberAsync(memberId));

    /// GET api/payments/history — all recorded payments
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
        => Ok(await _paymentService.GetPaymentHistoryAsync());

    /// GET api/payments/history/{memberId}
    [HttpGet("history/{memberId:guid}")]
    public async Task<IActionResult> GetHistoryByMember(Guid memberId)
        => Ok(await _paymentService.GetPaymentHistoryAsync(memberId));

    /// GET api/payments/summary — dashboard stats
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
        => Ok(await _paymentService.GetDashboardSummaryAsync());

    /// POST api/payments/record — record a payment against a schedule
    [HttpPost("record")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentDto dto)
    {
        var result = await _paymentService.RecordPaymentAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// POST api/payments/refresh-late — mark overdue schedules as Late
    [HttpPost("refresh-late")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> RefreshLate()
        => Ok(await _paymentService.RefreshLateStatusAsync());

    /// POST api/payments/generate-member-schedules — backfill missing PaymentSchedule rows
    /// for all memberships (from StartDate through EndDate). Safe to call multiple times.
    [HttpPost("generate-member-schedules")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> GenerateMemberSchedules()
        => Ok(await _paymentService.GenerateMissingMemberSchedulesAsync());
}
