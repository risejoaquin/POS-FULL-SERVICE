using PosDomain.Interfaces;
using PosDomain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosApplication.Interfaces.Server
{
    public interface IOrderService
    {
        Task<(bool isSuccess, string message, int? orderId)> CreateOrUpdateOrderAsync(Order order);
        Task<(List<Order> data, int page, int pageSize, int total)> GetOrdersAsync(int page, int pageSize);
        Task<Order?> GetOrderByIdAsync(int id);
    }
}
