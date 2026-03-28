using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Branches;
using GymSaaS.Application.Services;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchController : ControllerBase
{
    private readonly BranchService _branchService;

    public BranchController(BranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _branchService.GetAllAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving branches.", error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _branchService.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the branch.", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto)
    {
        try
        {
            return Ok(await _branchService.CreateAsync(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the branch.", error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchDto dto)
    {
        try
        {
            var result = await _branchService.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the branch.", error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _branchService.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the branch.", error = ex.Message });
        }
    }
}
