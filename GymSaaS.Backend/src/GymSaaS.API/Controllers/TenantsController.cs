using GymSaaS.Application.DTOs.Tenant;
using GymSaaS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.API.Controllers;
 
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterTenant([FromBody] RegisterTenantRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tenantId = await _tenantService.RegisterTenantAsync(request);
            return Ok(new { message = "Gym registered successfully", tenantId });
        }
        catch (Exception ex)
        {
            // Log exception here
            return StatusCode(500, new { message = "An error occurred while registering the gym." });
        }
    }
}
