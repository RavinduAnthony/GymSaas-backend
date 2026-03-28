using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Packages;
using GymSaaS.Application.Services;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PackageController : ControllerBase
{
    private readonly PackageService _packageService;

    public PackageController(PackageService packageService)
    {
        _packageService = packageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _packageService.GetAllAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving packages.", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create([FromBody] CreatePackageDto dto)
    {
        try
        {
            return Ok(await _packageService.CreateAsync(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the package.", error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePackageDto dto)
    {
        try
        {
            var result = await _packageService.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the package.", error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _packageService.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the package.", error = ex.Message });
        }
    }
}
