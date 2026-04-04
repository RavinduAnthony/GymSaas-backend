using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.ServiceSettings;
using GymSaaS.Application.Services;
using GymSaaS.Domain.Enums;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceSettingController : ControllerBase
{
    private readonly ServiceSettingService _service;

    public ServiceSettingController(ServiceSettingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _service.GetAllAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving service settings.", error = ex.Message });
        }
    }

    [HttpPut("{serviceType}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(ServiceType serviceType, [FromBody] UpdateServiceSettingDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(serviceType, dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the service setting.", error = ex.Message });
        }
    }
}
