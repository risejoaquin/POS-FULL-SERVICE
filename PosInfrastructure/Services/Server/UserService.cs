using PosDomain.Interfaces;
using Microsoft.EntityFrameworkCore;
using PosInfrastructure.Data.Server;
using PosDomain.Entities;
using System;
using System.Threading.Tasks;

using PosApplication.Interfaces.Server;

// PHASE 7C targeted server service nullability remediation: guarded nullable user payloads and username comparisons.
namespace PosInfrastructure.Services.Server
{
    public class UserService : IUserService
    {
        private readonly CentralDbContext _context;
        private readonly ITenantContext _tenantContext;

        public UserService(CentralDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<(bool isSuccess, string message, User? user)> CreateOrUpdateUserAsync(User? user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                return (false, "Invalid user payload.", null);
            }

            var tenantId = _tenantContext.GetTenantId();
            var usernameLower = user.Username.ToLowerInvariant();
            user.TenantId = tenantId;

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username != null && u.Username.ToLower() == usernameLower);
                
            if (existing == null)
            {
                user.Id = 0;
                user.CreatedAt = DateTime.UtcNow;
                user.PasswordHash = ResolvePasswordHash(user.Pin, user.PasswordHash);
                user.Pin = null;
                _context.Users.Add(user);
            }
            else
            {
                // Estrategia de Resolución de Conflictos (Last Write Wins)
                if (existing.LastUpdated > user.LastUpdated)
                {
                    return (false, "Conflicto de sincronización: la versión en el servidor es más reciente.", existing);
                }

                existing.Role = user.Role;
                existing.IsActive = user.IsActive;
                existing.LastUpdated = DateTime.UtcNow;
                var resolvedPasswordHash = ResolvePasswordHash(user.Pin, user.PasswordHash);
                if (!string.IsNullOrEmpty(resolvedPasswordHash)) existing.PasswordHash = resolvedPasswordHash;
                    
                _context.Users.Update(existing);
            }

            await _context.SaveChangesAsync();
            return (true, "Success", existing ?? user);
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            var tenantId = _tenantContext.GetTenantId();
            var usernameLower = username.ToLowerInvariant();
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username != null && u.Username.ToLower() == usernameLower);
            if (existing != null)
            {
                _context.Users.Remove(existing);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        private static string? ResolvePasswordHash(string? pin, string? passwordHash)
        {
            if (!string.IsNullOrWhiteSpace(pin))
            {
                return BCrypt.Net.BCrypt.HashPassword(pin);
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return passwordHash;
            }

            return IsBCryptHash(passwordHash) ? passwordHash : BCrypt.Net.BCrypt.HashPassword(passwordHash);
        }

        private static bool IsBCryptHash(string value)
        {
            return value.StartsWith("$2a$", StringComparison.Ordinal) ||
                   value.StartsWith("$2b$", StringComparison.Ordinal) ||
                   value.StartsWith("$2x$", StringComparison.Ordinal) ||
                   value.StartsWith("$2y$", StringComparison.Ordinal);
        }
    }
}
