using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Auth;
using GymSaaS.Application.Interfaces;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Register a new gym (tenant) and owner account</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterTenantDto dto)
    {
        try
        {
            var result = await _authService.RegisterTenantAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration.", error = ex.Message });
        }
    }

    /// <summary>Login and receive a JWT token</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.Success) return Unauthorized(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login.", error = ex.Message });
        }
    }
}
