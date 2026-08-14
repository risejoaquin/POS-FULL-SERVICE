using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PosApplication.DTOs.Local;
using PosDomain.Entities;

namespace PosApplication.Interfaces.Local
{
    public interface IReturnsService
    {
        Task<IReadOnlyList<Order>> SearchOrdersAsync(DateTime startDate, DateTime endDate, string searchQuery);
        Task<bool> HasActiveShiftAsync();
        Task<Order> GetOrderForReturnAsync(int id);
        Task<Order> ProcessFullReturnAsync(int orderId, string reason, string authorizedBy);
        Task<Order> ProcessPartialReturnAsync(int orderId, IReadOnlyList<ReturnItemRequest> itemsToReturn, string reason, string authorizedBy);
    }
}
