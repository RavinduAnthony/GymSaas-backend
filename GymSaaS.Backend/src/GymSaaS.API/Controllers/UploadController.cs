using GymSaaS.Application.Interfaces;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.API.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ITenantService _tenantService;
    private readonly ITenantProvider _tenantProvider;

    public UploadController(
        ICloudinaryService cloudinaryService,
        ITenantService tenantService,
        ITenantProvider tenantProvider)
    {
        _cloudinaryService = cloudinaryService;
        _tenantService = tenantService;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Upload the gym logo for the current tenant.
    /// Accepts multipart/form-data with a field named "file".
    /// Returns the Cloudinary secure URL.
    /// </summary>
    [HttpPost("logo")]
    public async Task<IActionResult> UploadLogo([FromForm] IFormFile file)
    {
        var tenantId = _tenantProvider.TenantId;

        if (tenantId == Guid.Empty)
            return Unauthorized(ApiResponse<string>.Fail("Tenant not identified."));

        try
        {
            await using var stream = file.OpenReadStream();
            var url = await _cloudinaryService.UploadImageAsync(stream, file.FileName, file.ContentType, tenantId.ToString(), "logo");
            await _tenantService.UpdateLogoAsync(tenantId, url);

            return Ok(ApiResponse<object>.Ok(new { url }, "Logo uploaded successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get the current logo URL for the tenant.
    /// </summary>
    [HttpGet("logo")]
    public async Task<IActionResult> GetLogo()
    {
        var tenantId = _tenantProvider.TenantId;

        if (tenantId == Guid.Empty)
            return Unauthorized(ApiResponse<string>.Fail("Tenant not identified."));

        var url = await _tenantService.GetLogoUrlAsync(tenantId);
        return Ok(ApiResponse<object>.Ok(new { url }));
    }

    /// <summary>
    /// Delete the gym logo for the current tenant from Cloudinary and clear the URL in the database.
    /// </summary>
    [HttpDelete("logo")]
    public async Task<IActionResult> DeleteLogo()
    {
        var tenantId = _tenantProvider.TenantId;

        if (tenantId == Guid.Empty)
            return Unauthorized(ApiResponse<string>.Fail("Tenant not identified."));

        try
        {
            await _cloudinaryService.DeleteImageAsync(tenantId.ToString(), "logo");
            await _tenantService.ClearLogoAsync(tenantId);
            return Ok(ApiResponse.Ok("Logo deleted successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }
}
