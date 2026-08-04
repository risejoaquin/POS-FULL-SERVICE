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
        try
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
        
        var licenseKey = $"VAL-{tenantId}-123";
        var licenseExists = await _dbContext.Licenses.IgnoreQueryFilters().AnyAsync(l => l.LicenseKey == licenseKey && l.TenantId == tenantId);
        if (!licenseExists)
        {
            _dbContext.Licenses.Add(new License {
                LicenseKey = licenseKey,
                TenantId = tenantId,
                Description = "Licencia Aprovisionada (Auto)",
                IsActive = true,
                MaxTerminals = 3,
                ValidUntil = DateTime.UtcNow.AddYears(1),
                CreatedAt = DateTime.UtcNow
            });
        }

        var productsExist = await _dbContext.Products.IgnoreQueryFilters().AnyAsync(p => p.TenantId == tenantId);
        if (!productsExist)
        {
            var p1 = new Product { Name = "Café Americano", Barcode = "75010001", Price = 35.00m, StockQuantity = 100, MinStockThreshold = 10, Category = "Bebidas", TenantId = tenantId };
            var p2 = new Product { Name = "Capuchino", Barcode = "75010002", Price = 45.00m, StockQuantity = 50, MinStockThreshold = 5, Category = "Bebidas", TenantId = tenantId };
            var p3 = new Product { Name = "Galleta de Chispas", Barcode = "75010003", Price = 15.00m, StockQuantity = 30, MinStockThreshold = 10, Category = "Postres", TenantId = tenantId };
            var p4 = new Product { Name = "Taco al Pastor", Barcode = "75010004", Price = 20.00m, StockQuantity = 200, MinStockThreshold = 20, Category = "Alimentos", TenantId = tenantId };
            var p5 = new Product { Name = "Refresco Cola 600ml", Barcode = "75010005", Price = 18.00m, StockQuantity = 100, MinStockThreshold = 20, Category = "Bebidas", TenantId = tenantId };
            
            _dbContext.Products.AddRange(p1, p2, p3, p4, p5);
            await _dbContext.SaveChangesAsync();

            var m1 = new ProductModifier { Name = "Tipo de Leche", Description = "Selecciona el tipo de leche", IsRequired = true, MinSelections = 1, MaxSelections = 1, TenantId = tenantId };
            var m2 = new ProductModifier { Name = "Endulzante", Description = "Agrega endulzante", IsRequired = false, MinSelections = 0, MaxSelections = 2, TenantId = tenantId };
            var m3 = new ProductModifier { Name = "Extras Pastor", Description = "Con todo o sin algo", IsRequired = false, MinSelections = 0, MaxSelections = 3, TenantId = tenantId };

            _dbContext.ProductModifiers.AddRange(m1, m2, m3);
            await _dbContext.SaveChangesAsync();

            _dbContext.ModifierOptions.AddRange(
                new ModifierOption { ProductModifierId = m1.Id, Name = "Entera", PriceAdjustment = 0, IsDefault = true, SortOrder = 1, TenantId = tenantId },
                new ModifierOption { ProductModifierId = m1.Id, Name = "Deslactosada", PriceAdjustment = 5, IsDefault = false, SortOrder = 2, TenantId = tenantId },
                new ModifierOption { ProductModifierId = m1.Id, Name = "Almendra", PriceAdjustment = 10, IsDefault = false, SortOrder = 3, TenantId = tenantId },
                new ModifierOption { ProductModifierId = m2.Id, Name = "Azúcar", PriceAdjustment = 0, IsDefault = true, SortOrder = 1, TenantId = tenantId },
                new ModifierOption { ProductModifierId = m2.Id, Name = "Splenda", PriceAdjustment = 0, IsDefault = false, SortOrder = 2, TenantId = tenantId },
                new ModifierOption { ProductModifierId = m3.Id, Name = "Sin Cebolla", PriceAdjustment = 0, IsDefault = false, SortOrder = 1, TenantId = tenantId },
                new ModifierOption { ProductModifierId = m3.Id, Name = "Sin Cilantro", PriceAdjustment = 0, IsDefault = false, SortOrder = 2, TenantId = tenantId },
                new ModifierOption { ProductModifierId = m3.Id, Name = "Extra Queso", PriceAdjustment = 10, IsDefault = false, SortOrder = 3, TenantId = tenantId }
            );

            _dbContext.ProductModifierLinks.AddRange(
                new ProductModifierLink { ProductId = p2.Id, ProductModifierId = m1.Id, SortOrder = 1, TenantId = tenantId },
                new ProductModifierLink { ProductId = p2.Id, ProductModifierId = m2.Id, SortOrder = 2, TenantId = tenantId },
                new ProductModifierLink { ProductId = p4.Id, ProductModifierId = m3.Id, SortOrder = 1, TenantId = tenantId }
            );
        }

        await _dbContext.SaveChangesAsync();
        return Ok(new { Message = "Tenant aprovisionado exitosamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = ex.Message, Inner = ex.InnerException?.Message, StackTrace = ex.StackTrace });
        }
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
