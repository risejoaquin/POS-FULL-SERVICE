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
    public class ShiftsController : ControllerBase
    {
        private readonly CentralDbContext _context;
        private readonly ITenantService _tenantService;

        public ShiftsController(CentralDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpPost]
        public async Task<IActionResult> SyncShift([FromBody] CashRegisterShift shift)
        {
            if (shift == null) return BadRequest();

            var tenantId = _tenantService.GetTenantId();
            shift.TenantId = tenantId;

            var existing = await _context.CashRegisterShifts
                .Include(s => s.Movements)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.OpenedAt == shift.OpenedAt && s.OpenedBy == shift.OpenedBy);

            if (existing == null)
            {
                shift.Id = 0; // Postgres ID
                if (shift.Movements != null)
                {
                    foreach (var mov in shift.Movements) { mov.Id = 0; mov.ShiftId = 0; mov.TenantId = tenantId; }
                }
                _context.CashRegisterShifts.Add(shift);
            }
            else
            {
                existing.ClosedAt = shift.ClosedAt;
                existing.ActualEndingCash = shift.ActualEndingCash;
                existing.ExpectedEndingCash = shift.ExpectedEndingCash;
                existing.Difference = shift.Difference;
                existing.LastUpdated = shift.LastUpdated;
                
                existing.ClosedBy = shift.ClosedBy;
                existing.IsClosed = shift.IsClosed;

                // Sync movements
                if (shift.Movements != null)
                {
                    existing.Movements ??= new List<CashMovement>();
                    foreach (var mov in shift.Movements)
                    {
                        if (!existing.Movements.Any(m => m.Type == mov.Type && m.Amount == mov.Amount && m.CreatedAt == mov.CreatedAt))
                        {
                        mov.Id = 0;
                        mov.ShiftId = existing.Id;
                        mov.TenantId = tenantId;
                        existing.Movements.Add(mov);
                    }
                }
                }
                
                _context.CashRegisterShifts.Update(existing);
            }

            await _context.SaveChangesAsync();
            return Ok(existing ?? shift);
        }
    }
}
