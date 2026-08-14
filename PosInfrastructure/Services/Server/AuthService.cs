using PosDomain.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PosInfrastructure.Data.Server;
using PosDomain.Entities;
using System.Linq;

using PosApplication.Interfaces.Server;

// PHASE 7C targeted AuthService nullability remediation: guarded password hashes, token claims, provision payloads and optional employee credentials.
// PHASE 7H AuthService remaining nullability hygiene applied: guarded login username normalization and nullable entity username comparison.
namespace PosInfrastructure.Services.Server
{
    public class AuthService : IAuthService
    {
        private readonly CentralDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(CentralDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<(bool IsSuccess, string Message, User? User, string? Token, string? RefreshToken)> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
            {
                return (false, "Username y Password son requeridos.", null, null, null);
            }

            var loginUsername = request.Username;
            var loginPassword = request.Password;
            var usernameLower = loginUsername.ToLowerInvariant();
            var user = await _dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == usernameLower && u.IsActive);

            if (user != null && !string.IsNullOrEmpty(user.PasswordHash) && BCrypt.Net.BCrypt.Verify(loginPassword, user.PasswordHash))
            {
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                return (true, "Login exitoso", user, token, refreshToken);
            }

            return (false, "Credenciales inválidas", null, null, null);
        }

        public async Task<(bool IsSuccess, string Message, string? Token, string? RefreshToken)> RefreshAsync(TokenRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.RefreshToken))
                return (false, "Invalid client request", null, null);

            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal == null) return (false, "Invalid access token or refresh token", null, null);

            var username = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var tenantId = principal.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(tenantId))
            {
                return (false, "Invalid access token or refresh token", null, null);
            }

            var user = await _dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Username == username && u.TenantId == tenantId);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return (false, "Invalid access token or refresh token", null, null);
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _dbContext.SaveChangesAsync();

            return (true, "Refrescado exitoso", newAccessToken, newRefreshToken);
        }

        public async Task<(bool IsSuccess, string Message, string LicenseKey)> ProvisionAsync(ProvisionRequest request)
        {
            if (request == null)
            {
                return (false, "Invalid provision payload", "");
            }

            var provisionKey = Environment.GetEnvironmentVariable("PROVISION_KEY") ?? _configuration["ProvisionKey"] ?? throw new InvalidOperationException("Missing PROVISION_KEY");
            if (request.ProvisionKey != provisionKey)
            {
                return (false, "ProvisionKey inválida", "");
            }
            
            var tenantId = request.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return (false, "TenantId requerido", "");
            }
            if (string.IsNullOrWhiteSpace(request.AdminUsername) || string.IsNullOrWhiteSpace(request.AdminPassword))
            {
                return (false, "Admin credentials required", "");
            }
            var adminUsername = request.AdminUsername;
            var adminPassword = request.AdminPassword;
            var adminUsernameLower = adminUsername.ToLowerInvariant();
            
            var adminExists = await _dbContext.Users.IgnoreQueryFilters().AnyAsync(u => u.TenantId == tenantId && u.Username != null && u.Username.ToLower() == adminUsernameLower);
            if (!adminExists)
            {
                _dbContext.Users.Add(new User {
                    Username = adminUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Role = "Admin",
                    TenantId = tenantId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                var adminUser = await _dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username != null && u.Username.ToLower() == adminUsernameLower);
                if (adminUser != null)
                {
                    adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
                    adminUser.Role = "Admin";
                }
            }
            
            if (!string.IsNullOrWhiteSpace(request.EmpUsername) && !string.IsNullOrWhiteSpace(request.EmpPassword))
            {
                var empUsername = request.EmpUsername;
                var empPassword = request.EmpPassword;
                var empUsernameLower = empUsername.ToLowerInvariant();
                var empExists = await _dbContext.Users.IgnoreQueryFilters().AnyAsync(u => u.TenantId == tenantId && u.Username != null && u.Username.ToLower() == empUsernameLower);
                if (!empExists)
                {
                    _dbContext.Users.Add(new User {
                        Username = empUsername,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(empPassword),
                        Role = "Cajero",
                        TenantId = tenantId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else 
                {
                    var empUser = await _dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username != null && u.Username.ToLower() == empUsernameLower);
                    if (empUser != null)
                    {
                        empUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(empPassword);
                        empUser.Role = "Cajero";
                    }
                }
            }
            
            var licenseKey = "VAL-" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
            var licenseExists = false; // Generate new secure random license key on each provision
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

            return (true, "Tenant aprovisionado exitosamente.", licenseKey);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("Missing JWT_KEY");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Missing Jwt:Issuer");
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Missing Jwt:Audience");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username ?? "unknown"),
                new Claim("TenantId", user.TenantId ?? "default"),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("Missing JWT_KEY");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = false 
            };

            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
