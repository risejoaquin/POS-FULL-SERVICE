using Microsoft.EntityFrameworkCore;
using PosApplication.DTOs.Local;
using PosApplication.Interfaces.Local;
using PosDomain.Entities;
using PosInfrastructure.Data.Local;

namespace PosInfrastructure.Services.Local;

public class LocalAuthService : ILocalAuthService
{
    private readonly PosDbContext _dbContext;

    public LocalAuthService(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LoginResult> AuthenticateLocalUserAsync(string username, string passwordOrPin)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordOrPin))
        {
            return new LoginResult { IsSuccess = false, Message = "Ingrese usuario y contraseña" };
        }

        var normalizedUsername = username.Trim().ToLowerInvariant();
        var users = await _dbContext.Users
            .IgnoreQueryFilters()
            .Where(u => u.IsActive && u.Username != null && u.Username.ToLower() == normalizedUsername)
            .ToListAsync();

        var user = users.FirstOrDefault(u => CredentialMatches(u, passwordOrPin));
        if (user != null)
        {
            return BuildUserLoginResult(user, $"local-token-{Guid.NewGuid()}", "Login local exitoso");
        }

        var hasAnyUsers = await _dbContext.Users.IgnoreQueryFilters().AnyAsync();
        if (!hasAnyUsers && normalizedUsername == "admin" && passwordOrPin == "admin")
        {
            return new LoginResult
            {
                IsSuccess = true,
                Message = "Login local admin inicial exitoso",
                Token = "local-token-admin",
                TenantId = "default",
                Username = "Admin",
                Role = "Admin",
                CurrentUserId = "admin"
            };
        }

        return new LoginResult { IsSuccess = false, Message = "Credenciales inválidas" };
    }

    public async Task<LoginResult> CacheCloudLoginAsync(string username, string passwordOrPin, string tenantId, string role)
    {
        var normalizedTenantId = string.IsNullOrWhiteSpace(tenantId) ? "default" : tenantId;
        var normalizedRole = string.IsNullOrWhiteSpace(role) ? "User" : role;
        var normalizedUsername = username.Trim();

        await MigrateLegacyLocalTenantDataAsync(normalizedTenantId);

        var existingUser = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == normalizedUsername.ToLowerInvariant());

        if (existingUser != null)
        {
            existingUser.Pin = passwordOrPin;
            existingUser.Role = normalizedRole;
            existingUser.TenantId = normalizedTenantId;
            existingUser.IsActive = true;
            await _dbContext.SaveChangesAsync();
            return BuildUserLoginResult(existingUser, null, "Login cloud cached locally");
        }

        var newUser = new User
        {
            Username = normalizedUsername,
            Pin = passwordOrPin,
            Role = normalizedRole,
            TenantId = normalizedTenantId,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();
        return BuildUserLoginResult(newUser, null, "Login cloud cached locally");
    }

    public async Task<bool> ValidateManagerOverrideAsync(string managerUsername, string managerPin)
    {
        if (string.IsNullOrWhiteSpace(managerPin))
        {
            return false;
        }

        // Legacy behavior preserved for this architecture-only phase.
        // Security hardening should remove this default override in a later phase.
        if (managerPin == "admin")
        {
            return true;
        }

        var usersQuery = _dbContext.Users
            .Where(u => u.IsActive && u.Role != null &&
                (u.Role.ToLower() == "admin" || u.Role.ToLower() == "manager"));

        if (!string.IsNullOrWhiteSpace(managerUsername))
        {
            var normalizedUsername = managerUsername.Trim().ToLowerInvariant();
            usersQuery = usersQuery.Where(u => u.Username != null && u.Username.ToLower() == normalizedUsername);
        }

        var users = await usersQuery.ToListAsync();
        return users.Any(u => CredentialMatches(u, managerPin));
    }

    public Task MigrateAdminIfNeededAsync()
    {
        // Kept for interface compatibility. Admin migration/hardening belongs to a later auth phase.
        return Task.CompletedTask;
    }

    private async Task MigrateLegacyLocalTenantDataAsync(string newTenantId)
    {
        const string oldTenantId = "TENANT_001";

        var productsToMigrate = await _dbContext.Products
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == oldTenantId || p.TenantId == "LOCAL")
            .ToListAsync();
        foreach (var product in productsToMigrate)
        {
            product.TenantId = newTenantId;
        }

        var suppliesToMigrate = await _dbContext.Supplies
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == oldTenantId || s.TenantId == "LOCAL")
            .ToListAsync();
        foreach (var supply in suppliesToMigrate)
        {
            supply.TenantId = newTenantId;
        }

        await _dbContext.SaveChangesAsync();
    }

    private static LoginResult BuildUserLoginResult(User user, string? token, string message)
    {
        return new LoginResult
        {
            IsSuccess = true,
            Message = message,
            User = user,
            Token = token,
            TenantId = string.IsNullOrWhiteSpace(user.TenantId) ? "default" : user.TenantId,
            Username = user.Username,
            Role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role,
            CurrentUserId = user.Id.ToString()
        };
    }

    private static bool CredentialMatches(User user, string credential)
    {
        if (!string.IsNullOrWhiteSpace(user.Pin) && user.Pin == credential)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return false;
        }

        if (user.PasswordHash == credential)
        {
            return true;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(credential, user.PasswordHash);
        }
        catch
        {
            return false;
        }
    }
}
