using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Trainers;
using GymSaaS.Application.Services;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainerController : ControllerBase
{
    private readonly TrainerService _trainerService;

    public TrainerController(TrainerService trainerService)
    {
        _trainerService = trainerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _trainerService.GetAllAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving trainers.", error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _trainerService.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the trainer.", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateTrainerDto dto)
    {
        try
        {
            return Ok(await _trainerService.CreateAsync(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the trainer.", error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTrainerDto dto)
    {
        try
        {
            var result = await _trainerService.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the trainer.", error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _trainerService.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the trainer.", error = ex.Message });
        }
    }
}
