using PosDomain;
using System.Threading.Tasks;

using PosDomain.Entities;
using PosDomain.Interfaces;

namespace PosApplication.UseCases.Orders
{
    public class CreateOrderUseCase
    {
        private readonly IOrderRepository _orderRepository;

        public CreateOrderUseCase(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<Order>> CreateOrderAsync(string tenantId, string createdById)
        {
            var order = new Order
            {
                TenantId = tenantId,
                CreatedById = createdById,
                Status = OrderStatus.Open,
                OrderDate = System.DateTime.UtcNow,
                ClientSideId = System.Guid.NewGuid().ToString()
            };
            
            await _orderRepository.AddAsync(order);
            return Result<Order>.Success(order);
        }

        public async Task<Result> AddItemToOrderAsync(int orderId, Product product, int quantity, decimal discount = 0, string notes = "")
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return Result.Failure("Order not found.");

            var addResult = order.AddItem(product, quantity, discount, notes);
            if (!addResult.IsSuccess) return addResult;

            await _orderRepository.UpdateAsync(order);
            return Result.Success();
        }

        public async Task<Result> CompleteOrderAsync(int orderId, string authorizedBy)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return Result.Failure("Order not found.");

            var completeResult = order.Complete(authorizedBy);
            if (!completeResult.IsSuccess) return completeResult;

            await _orderRepository.UpdateAsync(order);
            return Result.Success();
        }
    }
}
