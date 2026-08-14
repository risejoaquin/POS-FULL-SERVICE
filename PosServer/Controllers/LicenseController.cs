using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PosDomain.Entities;
using PosApplication.Interfaces.Server;
using PosInfrastructure.Services.Server;
using PosDomain.Interfaces;
using System.Threading.Tasks;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosServer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LicenseController : ControllerBase
{
    private readonly ILicenseService _licenseService;

    public LicenseController(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpPost("validate")]
    [EnableRateLimiting("LoginPolicy")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateLicense([FromBody] LicenseRequest request)
    {
        var (isValid, error, license) = await _licenseService.ValidateLicenseAsync(request.LicenseKey);
        if (!isValid)
        {
            return Ok(new { IsValid = false, Error = error });
        }
        
        return Ok(new 
        { 
            IsValid = true, 
            MaxTerminals = license!.MaxTerminals,
            ValidUntil = license.ValidUntil,
            TenantId = license.TenantId
        });
    }

    [HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateLicense([FromBody] GenerateLicenseRequest request)
    {
        var newLicense = await _licenseService.GenerateLicenseAsync(
            request.TenantId ?? string.Empty,
            request.Description ?? string.Empty,
            request.MaxTerminals,
            request.DurationDays
        );

        return Ok(new { 
            Message = "Licencia generada exitosamente.",
            LicenseKey = newLicense.LicenseKey,
            ValidUntil = newLicense.ValidUntil,
            TenantId = newLicense.TenantId
        });
    }
}

public class LicenseRequest
{
    public string LicenseKey { get; set; } = string.Empty;
}

public class GenerateLicenseRequest
{
    public string? TenantId { get; set; }
    public string? Description { get; set; }
    public int MaxTerminals { get; set; }
    public int DurationDays { get; set; }
}
