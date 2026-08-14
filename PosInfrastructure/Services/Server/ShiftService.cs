using PosDomain.Interfaces;
using Microsoft.EntityFrameworkCore;
using PosInfrastructure.Data.Server;
using PosDomain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PosApplication.Interfaces.Server;

namespace PosInfrastructure.Services.Server
{
    public class ShiftService : IShiftService
    {
        private readonly CentralDbContext _context;
        private readonly ITenantContext _tenantContext;

        public ShiftService(CentralDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<(bool isSuccess, string message, CashRegisterShift shift)> SyncShiftAsync(CashRegisterShift shift)
        {
            var tenantId = _tenantContext.GetTenantId();
            shift.TenantId = tenantId;

            var existing = await _context.CashRegisterShifts
                .Include(s => s.Movements)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.OpenedAt == shift.OpenedAt && s.OpenedBy == shift.OpenedBy);


            if (shift.IsClosed)
            {
                if (shift.ActualEndingCash == null || shift.ExpectedEndingCash == null)
                {
                    return (false, "Para cerrar la caja es obligatorio proveer el efectivo esperado y el efectivo real.", existing ?? shift);
                }
                
                var expectedDiff = shift.ActualEndingCash.Value - shift.ExpectedEndingCash.Value;
                if (shift.Difference != expectedDiff)
                {
                    return (false, $"Error de reconciliación: la diferencia matemática es {expectedDiff}, pero se reportó {shift.Difference}.", existing ?? shift);
                }
            }

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
                // Estrategia de Resolución de Conflictos (Last Write Wins)
                if (existing.LastUpdated > shift.LastUpdated)
                {
                    return (false, "Conflicto de sincronización: la versión en el servidor es más reciente.", existing);
                }

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
            return (true, "Success", existing ?? shift);
        }
    }
}
