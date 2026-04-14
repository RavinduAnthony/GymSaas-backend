using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymSaaS.Application.DTOs.Roles;
using GymSaaS.Application.Services;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly RoleService _service;

    public RoleController(RoleService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return result.Success ? Ok(result) : StatusCode(500, result);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{roleName}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(string roleName)
    {
        var result = await _service.DeleteAsync(roleName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{roleName}/permissions")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> GetPermissions(string roleName)
    {
        var result = await _service.GetPermissionsAsync(roleName);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{roleName}/permissions")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> UpdatePermissions(string roleName, [FromBody] UpdateRolePermissionsDto dto)
    {
        var result = await _service.UpdatePermissionsAsync(roleName, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Returns all permission keys where IsAllowed=true for the calling user's role.
    /// Owner always gets all permissions regardless of DB rows.
    /// </summary>
    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        // Owner always has full access
        if (roleClaim == "Owner")
        {
            var allKeys = GymSaaS.Shared.UiPermissionKeys.All;
            return Ok(new { success = true, data = allKeys });
        }

        // For custom-role users, look up by their CustomRole claim if present;
        // otherwise fall back to the system role claim.
        var customRoleClaim = User.FindFirstValue("CustomRole");
        var lookupRole = string.IsNullOrEmpty(customRoleClaim) ? roleClaim : customRoleClaim;

        var result = await _service.GetMyPermissionsAsync(lookupRole);
        return result.Success ? Ok(result) : StatusCode(500, result);
    }
}
