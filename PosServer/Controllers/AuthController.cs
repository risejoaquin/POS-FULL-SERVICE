using Microsoft.AspNetCore.Mvc;
using PosDomain.Entities;
using PosApplication.Interfaces.Server;
using PosInfrastructure.Services.Server;
using PosDomain.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.RateLimiting;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosServer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("DefaultPolicy")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (isSuccess, message, user, token, refreshToken) = await _authService.LoginAsync(request);
        if (isSuccess && user != null)
        {
            return Ok(new { 
                Token = token, 
                RefreshToken = refreshToken,
                TenantId = user.TenantId ?? "default",
                user.Id, 
                user.Username, 
                user.Role 
            });
        }
        return Unauthorized(new { Message = message });
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Refresh([FromBody] TokenRequest request)
    {
        var (isSuccess, message, token, refreshToken) = await _authService.RefreshAsync(request);
        if (isSuccess)
        {
            return Ok(new { Token = token, RefreshToken = refreshToken });
        }
        return BadRequest(message);
    }

    [HttpPost("provision")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> ProvisionTenant([FromBody] ProvisionRequest request)
    {
        var (isSuccess, message, licenseKey) = await _authService.ProvisionAsync(request);
        if (isSuccess)
        {
            return Ok(new { Message = message, LicenseKey = licenseKey });
        }
        return BadRequest(new { Message = message });
    }
}
