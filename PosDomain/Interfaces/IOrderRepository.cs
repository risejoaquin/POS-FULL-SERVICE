using PosDomain;
using System.Threading.Tasks;
using PosDomain.Entities;

namespace PosDomain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, string tenantId);
    }
}
