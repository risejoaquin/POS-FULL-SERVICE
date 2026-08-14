using PosDomain.Interfaces;
using Microsoft.EntityFrameworkCore;
using PosInfrastructure.Data.Server;
using PosDomain.Entities;
using System.Threading.Tasks;

using PosApplication.Interfaces.Server;

namespace PosInfrastructure.Services.Server
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly CentralDbContext _context;
        private readonly ITenantContext _tenantContext;

        public InventoryMovementService(CentralDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<InventoryMovement> SyncMovementAsync(InventoryMovement movement)
        {
            var tenantId = _tenantContext.GetTenantId();
            movement.TenantId = tenantId;
            
            var existing = await _context.InventoryMovements
                .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Reference == movement.Reference && m.ProductId == movement.ProductId && m.SupplyId == movement.SupplyId && m.MovementDate == movement.MovementDate);
                
            if (existing == null)
            {
                movement.Id = 0; // Let Postgres generate ID
                _context.InventoryMovements.Add(movement);
                await _context.SaveChangesAsync();
                return movement;
            }
            
            return existing;
        }
    }
}
