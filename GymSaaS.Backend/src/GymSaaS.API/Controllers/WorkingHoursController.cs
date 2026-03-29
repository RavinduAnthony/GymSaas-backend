using GymSaaS.Application.DTOs.Settings;
using GymSaaS.Application.Services;
using GymSaaS.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkingHoursController : ControllerBase
{
    private readonly WorkingHoursService _service;

    public WorkingHoursController(WorkingHoursService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedule()
    {
        try
        {
            var result = await _service.GetScheduleAsync();
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Server error: {ex.Message}"));
        }
    }

    [HttpPut]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> UpsertSchedule([FromBody] List<UpsertWorkingHoursItemDto> items)
    {
        try
        {
            var result = await _service.UpsertScheduleAsync(items);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Server error: {ex.Message}"));
        }
    }
}
