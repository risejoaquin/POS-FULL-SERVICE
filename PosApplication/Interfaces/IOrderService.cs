using System.Collections.Generic;
using System.Threading.Tasks;
using PosDomain.Entities;
using PosDomain;

namespace PosApplication.Interfaces
{
    public interface IOrderService
    {
        Task<Result<Order>> CreateDraftOrderAsync(string customerName, string tenantId, string createdById);
        Result TransitionTo(Order order, OrderStatus nextStatus);
        Task<Result> CheckoutAsync(Order order, List<(OrderItem item, Product product)> cartItems, decimal totalPaid, decimal change, string paymentDetails);
    }
}
