using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosServer.Data;
using PosServer.Models;
using PosServer.Services;

namespace PosServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly CentralDbContext _context;
        private readonly ITenantService _tenantService;

        public SyncController(CentralDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("changes")]
        public async Task<IActionResult> GetChanges([FromQuery] string? since)
        {
            var tenantId = _tenantService.GetTenantId();
            DateTime sinceDateTime = DateTime.MinValue;

            if (!string.IsNullOrEmpty(since) && DateTime.TryParse(since, out var parsed))
            {
                sinceDateTime = parsed.ToUniversalTime(); // assuming UTC in db
            }

            var products = await _context.Products.AsNoTracking().Where(p => p.TenantId == tenantId && p.LastUpdated >= sinceDateTime).ToListAsync();
            var users = await _context.Users.AsNoTracking().Where(u => u.TenantId == tenantId && u.LastUpdated >= sinceDateTime).ToListAsync();
            var shifts = await _context.CashRegisterShifts.Include(s => s.Movements).AsNoTracking().Where(s => s.TenantId == tenantId && s.LastUpdated >= sinceDateTime).ToListAsync();
            var orders = await _context.Orders.Include(o => o.Items).AsNoTracking().Where(o => o.TenantId == tenantId && o.LastUpdated >= sinceDateTime).ToListAsync();

            return Ok(new {
                products,
                users,
                shifts,
                orders
            });
        }
    }
}
