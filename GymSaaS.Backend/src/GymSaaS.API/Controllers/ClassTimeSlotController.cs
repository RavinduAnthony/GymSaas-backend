using GymSaaS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassTimeSlotController : ControllerBase
{
    private readonly IClassTimeSlotService _service;

    public ClassTimeSlotController(IClassTimeSlotService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns all occupied 30-min time slots for a specific branch.
    /// Pass excludeClassId when editing a class to omit its own slots.
    /// </summary>
    [HttpGet("branch/{branchId:guid}")]
    public async Task<IActionResult> GetByBranch(
        Guid branchId, [FromQuery] Guid? excludeClassId = null)
    {
        var result = await _service.GetByBranchAsync(branchId, excludeClassId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
