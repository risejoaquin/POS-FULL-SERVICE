using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PosApplication.Interfaces;
using PosDomain;
using PosDomain.Interfaces;
using PosDomain.Entities;

namespace PosApplication.UseCases.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public Task<Result<Order>> CreateDraftOrderAsync(string customerName, string tenantId, string createdById)
        {
            var order = new Order
            {
                Status = OrderStatus.Draft,
                OrderDate = DateTime.Now,
                CustomerName = customerName,
                TenantId = tenantId,
                CreatedById = createdById,
                IsReturned = false
            };
            // PHASE 7I ClientOrderService async hygiene applied: preserve Task-based contract without async state machine.
            return Task.FromResult(Result<Order>.Success(order));
        }

        public Result TransitionTo(Order order, OrderStatus nextStatus)
        {
            if (!IsValidTransition(order.Status, nextStatus))
            {
                return Result.Failure($"Invalid transition from {order.Status} to {nextStatus}");
            }
            order.Status = nextStatus;
            return Result.Success();
        }

        private bool IsValidTransition(OrderStatus current, OrderStatus next)
        {
            return current switch
            {
                OrderStatus.Draft => next == OrderStatus.Open,
                OrderStatus.Open => next == OrderStatus.Paid || next == OrderStatus.Cancelled,
                OrderStatus.Paid => next == OrderStatus.Closed || next == OrderStatus.Refunded,
                OrderStatus.Closed => next == OrderStatus.Refunded,
                _ => false
            };
        }

        public async Task<Result> CheckoutAsync(
            Order order, 
            List<(OrderItem item, Product product)> cartItems, 
            decimal totalPaid, 
            decimal change,
            string paymentDetails)
        {
            var transitionOpen = TransitionTo(order, OrderStatus.Open);
            if (!transitionOpen.IsSuccess) return transitionOpen;

            order.PaymentDetails = paymentDetails;
            
            foreach (var (item, product) in cartItems)
            {
                var addResult = order.AddItem(product, item.Quantity, item.Discount, item.Notes);
                if (!addResult.IsSuccess) return addResult;

                var decreaseStock = product.DecreaseStock(item.Quantity);
                if (!decreaseStock.IsSuccess) return Result.Failure("No hay suficiente stock.");
            }

            var transitionPaid = TransitionTo(order, OrderStatus.Paid);
            if (!transitionPaid.IsSuccess) return transitionPaid;

            // In a real app we would wrap this in an IUnitOfWork or transaction across repositories
            await _orderRepository.AddAsync(order);
            foreach (var (_, product) in cartItems)
            {
                await _productRepository.UpdateAsync(product);
            }
            
            var transitionClosed = TransitionTo(order, OrderStatus.Closed);
            if (!transitionClosed.IsSuccess) return transitionClosed;
            
            await _orderRepository.UpdateAsync(order);
            
            return Result.Success();
        }
    }
}
