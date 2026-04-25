using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Auth;
using GymSaaS.Application.DTOs.Users;
using GymSaaS.Application.Services;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _service;

    public UserController(UserService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return result.Success ? Ok(result) : StatusCode(500, result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Reset password for a user (clears IsTemporaryPassword flag).</summary>
    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return BadRequest(new { success = false, message = "Password must be at least 6 characters." });

        var result = await _service.ResetPasswordAsync(id, dto.NewPassword);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
