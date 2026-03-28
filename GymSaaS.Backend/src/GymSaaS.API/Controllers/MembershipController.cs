using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Memberships;
using GymSaaS.Application.Services;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembershipController : ControllerBase
{
    private readonly MembershipService _membershipService;

    public MembershipController(MembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    [HttpGet("member/{memberId:guid}")]
    public async Task<IActionResult> GetByMember(Guid memberId)
    {
        try
        {
            return Ok(await _membershipService.GetByMemberAsync(memberId));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving memberships.", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> Create([FromBody] CreateMembershipDto dto)
    {
        try
        {
            return Ok(await _membershipService.CreateAsync(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the membership.", error = ex.Message });
        }
    }

    [HttpPost("renew")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> Renew([FromBody] RenewMembershipDto dto)
    {
        try
        {
            var result = await _membershipService.RenewAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while renewing the membership.", error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMembershipDto dto)
    {
        try
        {
            var result = await _membershipService.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the membership.", error = ex.Message });
        }
    }

    [HttpGet("payments/{memberId:guid}")]
    public async Task<IActionResult> GetPayments(Guid memberId)
    {
        try
        {
            return Ok(await _membershipService.GetPaymentsByMemberAsync(memberId));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving payments.", error = ex.Message });
        }
    }
}
