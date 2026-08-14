using System.Collections.Generic;
using System.Threading.Tasks;
using PosDomain.Entities;
using PosApplication.DTOs.Local;

namespace PosApplication.Interfaces.Local
{
    public interface ILocalOrderService
    {
        Task<Order> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<Order> CreateOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task<Order> SaveOrderAsync(Order order);
        Task DeleteOrderAsync(int id);
        Task<bool> HasActiveShiftAsync(string tenantId);
        Task<CheckoutResult> ProcessCheckoutAsync(CheckoutRequest request);
    }
}
