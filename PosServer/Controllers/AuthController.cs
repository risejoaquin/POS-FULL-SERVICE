using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PosServer.Data;
using Microsoft.EntityFrameworkCore;
using PosServer.Models;
using PosServer.Services;

namespace PosServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly CentralDbContext _dbContext;
    private readonly ITenantService _tenantService;

    public AuthController(IConfiguration configuration, CentralDbContext dbContext, ITenantService tenantService)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    [HttpPost("login")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tenantId = _tenantService.GetTenantId();
        
        try 
        {
            // First we try to match by exact TenantId (if provided via Token, but this is a login so usually no token).
            // But we'll try it as requested. If tenantId is empty, it might mean the request has no token.
            // If the user's snippet insists on this, we'll include it.
            // Wait, their snippet used `&& u.Pin == request.Pin`. Our LoginRequest has `Password`.
            
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
            {
                return BadRequest(new { Message = "Username y Password son requeridos." });
            }
            
            var usernameLower = request.Username.ToLower();
            var user = await _dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => (string.IsNullOrEmpty(tenantId) || u.TenantId == tenantId)
                                       && u.Username.ToLower() == usernameLower 
                                       && u.IsActive);
                        
            if (user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                var token = GenerateJwtToken(user.Username, user.TenantId);
                // Return exactly what the user requested: { user.Id, user.Username, user.Role, user.TenantId }
                // PLUS the Token so it continues to work with other clients
                return Ok(new { 
                    Token = token, 
                    TenantId = user.TenantId ?? "default",
                    user.Id, 
                    user.Username, 
                    user.Role 
                });
            }
            return Unauthorized(new { Message = "Credenciales inválidas o usuario no activo." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Error en la autenticación", Details = ex.Message });
        }
    }

    [HttpPost("provision")]
    public async Task<IActionResult> Provision([FromBody] ProvisionRequest request)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"] ?? "super_secret_fallback_jwt_key_1234567890";
        if (request.ProvisionKey != jwtKey)
        {
            return Unauthorized(new { Message = "ProvisionKey inválida" });
        }
        
        var tenantId = request.TenantId;
        if (string.IsNullOrEmpty(tenantId))
        {
            return BadRequest(new { Message = "TenantId requerido" });
        }
        
        var adminExists = await _dbContext.Users.IgnoreQueryFilters().AnyAsync(u => u.TenantId == tenantId && u.Username.ToLower() == request.AdminUsername.ToLower());
        if (!adminExists)
        {
            _dbContext.Users.Add(new User {
                Username = request.AdminUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
                Role = "Admin",
                TenantId = tenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            var adminUser = await _dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username.ToLower() == request.AdminUsername.ToLower());
            if (adminUser != null)
            {
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword);
                adminUser.Role = "Admin";
            }
        }
        
        if (!string.IsNullOrEmpty(request.EmpUsername))
        {
            var empExists = await _dbContext.Users.IgnoreQueryFilters().AnyAsync(u => u.TenantId == tenantId && u.Username.ToLower() == request.EmpUsername.ToLower());
            if (!empExists)
            {
                _dbContext.Users.Add(new User {
                    Username = request.EmpUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.EmpPassword),
                    Role = "Cajero",
                    TenantId = tenantId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else 
            {
                var empUser = await _dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username.ToLower() == request.EmpUsername.ToLower());
                if (empUser != null)
                {
                    empUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.EmpPassword);
                    empUser.Role = "Cajero";
                }
            }
        }
        
        await _dbContext.SaveChangesAsync();
        return Ok(new { Message = "Tenant aprovisionado exitosamente." });
    }
    private string GenerateJwtToken(string username, string tenantId)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"] ?? "super_secret_fallback_jwt_key_1234567890";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "PosServer";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "PosClient";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username ?? "unknown"),
            new Claim("TenantId", tenantId ?? "default"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
