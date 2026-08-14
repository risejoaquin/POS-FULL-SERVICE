using PosInfrastructure.Data.Local;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosDomain.Interfaces;
using PosDomain.Entities;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosInfrastructure.Repositories.Local
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(PosDbContext context) : base(context)
        {
        }

        public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, string tenantId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey && o.TenantId == tenantId);
        }
    }
}
