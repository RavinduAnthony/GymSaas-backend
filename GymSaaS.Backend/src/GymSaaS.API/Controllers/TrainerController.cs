using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Trainers;
using GymSaaS.Application.Interfaces;
using GymSaaS.Application.Services;
using GymSaaS.Domain.Interfaces;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainerController : ControllerBase
{
    private readonly TrainerService _trainerService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ITenantProvider _tenantProvider;

    public TrainerController(
        TrainerService trainerService,
        ICloudinaryService cloudinaryService,
        ITenantProvider tenantProvider)
    {
        _trainerService = trainerService;
        _cloudinaryService = cloudinaryService;
        _tenantProvider = tenantProvider;
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateTrainerDto dto, [FromForm] IFormFile? photo)
    {
        try
        {
            if (photo != null)
            {
                await using var stream = photo.OpenReadStream();
                dto.PhotoUrl = await _cloudinaryService.UploadImageAsync(
                    stream, photo.FileName, photo.ContentType,
                    _tenantProvider.TenantId.ToString(), "trainers", Guid.NewGuid().ToString("N"));
            }

            return Ok(await _trainerService.CreateAsync(dto));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the trainer.", error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateTrainerDto dto, [FromForm] IFormFile? photo)
    {
        try
        {
            if (photo != null)
            {
                await using var stream = photo.OpenReadStream();
                dto.PhotoUrl = await _cloudinaryService.UploadImageAsync(
                    stream, photo.FileName, photo.ContentType,
                    _tenantProvider.TenantId.ToString(), "trainers", Guid.NewGuid().ToString("N"));
            }

            var result = await _trainerService.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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

    [HttpDelete("{id:guid}/photo")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> DeletePhoto(Guid id)
    {
        try
        {
            var result = await _trainerService.DeletePhotoAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the trainer photo.", error = ex.Message });
        }
    }
}
