using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSaaS.Application.DTOs.Members;
using GymSaaS.Application.Interfaces;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MemberController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MemberController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _memberService.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving members.", error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _memberService.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the member.", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
    {
        try
        {
            var result = await _memberService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the member.", error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberDto dto)
    {
        try
        {
            var result = await _memberService.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the member.", error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _memberService.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the member.", error = ex.Message });
        }
    }
}
