using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Payments;
using GymSaaS.Application.Services;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicePaymentsController : ControllerBase
{
    private readonly ServicePaymentService _service;

    public ServicePaymentsController(ServicePaymentService service)
    {
        _service = service;
    }

    /// GET api/servicepayments/schedules — all service payment schedules (auto-generates this month's)
    [HttpGet("schedules")]
    public async Task<IActionResult> GetAllSchedules()
        => Ok(await _service.GetAllSchedulesAsync());

    /// GET api/servicepayments/history — all recorded service payments
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
        => Ok(await _service.GetHistoryAsync());

    /// POST api/servicepayments/generate — manually trigger schedule generation for current month
    [HttpPost("generate")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> GenerateSchedules([FromBody] GenerateServiceSchedulesDto? dto)
        => Ok(await _service.GenerateMonthlySchedulesAsync(dto?.ServiceType));

    /// POST api/servicepayments/record — record payment against a schedule
    [HttpPost("record")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> RecordPayment([FromBody] RecordServicePaymentDto dto)
    {
        var result = await _service.RecordPaymentAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// POST api/servicepayments/refresh-late — mark overdue schedules as Late
    [HttpPost("refresh-late")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> RefreshLate()
        => Ok(await _service.RefreshLateStatusAsync());
}
