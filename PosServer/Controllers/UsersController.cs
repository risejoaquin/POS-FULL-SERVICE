using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosServer.Data;
using PosServer.Models;
using PosServer.Services;
using System.Security.Cryptography;
using System.Text;

namespace PosServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly CentralDbContext _context;
        private readonly ITenantService _tenantService;

        public UsersController(CentralDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username)) return BadRequest("Invalid user payload.");

            var tenantId = _tenantService.GetTenantId();
            user.TenantId = tenantId;

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username.ToLower() == user.Username.ToLower());
            
            // Si el cliente envía 'Pin', lo guardamos como 'PasswordHash' en la nube para consistencia 
            // aunque el modelo PosCore.Models.User usa 'Pin' y PosServer.Models.User usa 'PasswordHash'.
            // Vamos a permitir que el cliente envíe una estructura que sea serializada/deserializada adecuadamente
            
            if (existing == null)
            {
                user.Id = 0;
                user.CreatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(user.Pin)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Pin);
                else if (!string.IsNullOrEmpty(user.PasswordHash)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                _context.Users.Add(user);
            }
            else
            {
                existing.Role = user.Role;
                existing.IsActive = user.IsActive;
                existing.LastUpdated = DateTime.UtcNow;
                // Asumiremos que si viene password, lo actualizamos.
                if (!string.IsNullOrEmpty(user.PasswordHash)) existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                if (!string.IsNullOrEmpty(user.Pin)) existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Pin);
                
                _context.Users.Update(existing);
            }

            await _context.SaveChangesAsync();
            return Ok(existing ?? user);
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var tenantId = _tenantService.GetTenantId();
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username.ToLower() == username.ToLower());
            if (existing != null)
            {
                _context.Users.Remove(existing);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
