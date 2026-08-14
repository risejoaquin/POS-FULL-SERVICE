using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosApplication.DTOs.Local;
using PosApplication.Interfaces.Local;
using PosDomain.Entities;
using PosInfrastructure.Data.Local;

namespace PosInfrastructure.Services.Local
{
    public class ReturnsService : IReturnsService
    {
        private const decimal TaxRate = 0.16m;

        private readonly PosDbContext _dbContext;
        private readonly IInventoryService _inventoryService;

        public ReturnsService(PosDbContext dbContext, IInventoryService inventoryService)
        {
            _dbContext = dbContext;
            _inventoryService = inventoryService;
        }

        public async Task<IReadOnlyList<Order>> SearchOrdersAsync(DateTime startDate, DateTime endDate, string searchQuery)
        {
            var query = _dbContext.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.OrderDate.Date >= startDate.Date && o.OrderDate.Date <= endDate.Date);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                if (int.TryParse(searchQuery, out var orderId))
                {
                    query = query.Where(o => o.Id == orderId);
                }
                else
                {
                    query = query.Where(o => EF.Functions.Like(o.CustomerName ?? string.Empty, $"%{searchQuery}%"));
                }
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<bool> HasActiveShiftAsync()
        {
            return await _dbContext.CashRegisterShifts.AnyAsync(s => !s.IsClosed);
        }

        public async Task<Order> GetOrderForReturnAsync(int id)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order ?? throw new InvalidOperationException($"No se encontró la orden #{id}.");
        }

        public async Task<Order> ProcessFullReturnAsync(int orderId, string reason, string authorizedBy)
        {
            var activeShift = await GetActiveShiftOrThrowAsync();
            var order = await GetTrackedOrderForReturnAsync(orderId);

            if (order.IsReturned)
            {
                throw new InvalidOperationException("Esta orden ya fue devuelta.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                order.IsReturned = true;
                var orderManager = new OrderManagementService(_dbContext, _inventoryService);
                orderManager.TransitionState(order, OrderStatus.Refunded);

                order.ReturnReason = reason;
                order.AuthorizedBy = authorizedBy;
                order.LastUpdated = DateTime.Now;

                AddCashMovementIfCashPayment(activeShift, order, Math.Min(order.TotalAmount, GetCashPaid(order.PaymentDetails)), $"Devolución Orden #{order.Id} - {reason}", authorizedBy, order.TenantId);

                foreach (var item in order.Items)
                {
                    await _inventoryService.RegisterReturnAsync(item.ProductId, item.Quantity, $"Devolución completa orden #{order.Id}", order.TenantId);
                }

                AddOutboxMessage(order.TenantId, order.Id, "OrderReturned", new { OrderId = order.Id, TotalAmount = order.TotalAmount });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                _dbContext.ChangeTracker.Clear();
                throw;
            }
        }

        public async Task<Order> ProcessPartialReturnAsync(int orderId, IReadOnlyList<ReturnItemRequest> itemsToReturn, string reason, string authorizedBy)
        {
            if (itemsToReturn == null || itemsToReturn.Count == 0)
            {
                throw new InvalidOperationException("Debe seleccionar al menos un artículo para devolución parcial.");
            }

            var activeShift = await GetActiveShiftOrThrowAsync();
            var order = await GetTrackedOrderForReturnAsync(orderId);

            if (order.IsReturned)
            {
                throw new InvalidOperationException("Esta orden ya fue devuelta en su totalidad.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                decimal totalRefund = 0;

                foreach (var returnItem in itemsToReturn)
                {
                    var originalItem = order.Items.FirstOrDefault(i => i.Id == returnItem.OrderItemId)
                        ?? throw new InvalidOperationException($"No se encontró el artículo #{returnItem.OrderItemId} en la orden #{order.Id}.");

                    if (returnItem.Quantity <= 0 || returnItem.Quantity > originalItem.Quantity)
                    {
                        throw new InvalidOperationException($"Cantidad inválida para {originalItem.Product?.Name ?? originalItem.ProductBarcode}.");
                    }

                    totalRefund += originalItem.UnitPrice * returnItem.Quantity;
                    originalItem.Quantity -= returnItem.Quantity;

                    await _inventoryService.RegisterReturnAsync(originalItem.ProductId, returnItem.Quantity, $"Devolución parcial orden #{order.Id}", order.TenantId);
                }

                var itemsToRemove = order.Items.Where(i => i.Quantity <= 0).ToList();
                foreach (var itemToRemove in itemsToRemove)
                {
                    _dbContext.OrderItems.Remove(itemToRemove);
                    order.Items.Remove(itemToRemove);
                }

                order.SubTotal = order.Items.Sum(i => i.SubTotal);
                order.TaxAmount = order.SubTotal * TaxRate;
                order.TotalAmount = order.SubTotal + order.TaxAmount;

                order.ReturnReason = reason + " (Devolución Parcial)";
                order.AuthorizedBy = authorizedBy;
                order.LastUpdated = DateTime.Now;

                var orderManager = new OrderManagementService(_dbContext, _inventoryService);
                if (order.Items.Count == 0)
                {
                    order.IsReturned = true;
                    orderManager.TransitionState(order, OrderStatus.Refunded);
                }

                AddCashMovementIfCashPayment(activeShift, order, Math.Min(totalRefund, GetCashPaid(order.PaymentDetails)), $"Devolución Parcial Orden #{order.Id} - {reason}", authorizedBy, order.TenantId);

                AddOutboxMessage(order.TenantId, order.Id, "OrderPartiallyReturned", new { OrderId = order.Id, RefundedAmount = totalRefund });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                _dbContext.ChangeTracker.Clear();
                throw;
            }
        }

        private async Task<CashRegisterShift> GetActiveShiftOrThrowAsync()
        {
            var activeShift = await _dbContext.CashRegisterShifts.FirstOrDefaultAsync(s => !s.IsClosed);
            return activeShift ?? throw new InvalidOperationException("No hay un turno abierto. Por favor, abra un turno antes de realizar devoluciones.");
        }

        private async Task<Order> GetTrackedOrderForReturnAsync(int orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return order ?? throw new InvalidOperationException($"No se encontró la orden #{orderId}.");
        }

        private static decimal GetCashPaid(string? paymentDetails)
        {
            if (string.IsNullOrWhiteSpace(paymentDetails) || !paymentDetails.Contains("Efectivo"))
            {
                return 0;
            }

            decimal cashPaid = 0;
            var payments = paymentDetails.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var payment in payments)
            {
                if (payment.StartsWith("Efectivo: "))
                {
                    var amountText = payment.Substring("Efectivo: ".Length);
                    if (decimal.TryParse(amountText, NumberStyles.Currency, null, out var amount))
                    {
                        cashPaid += amount;
                    }
                }
            }

            return cashPaid;
        }

        private void AddCashMovementIfCashPayment(CashRegisterShift activeShift, Order order, decimal refundAmount, string reason, string authorizedBy, string tenantId)
        {
            if (refundAmount <= 0 || string.IsNullOrWhiteSpace(order.PaymentDetails) || !order.PaymentDetails.Contains("Efectivo"))
            {
                return;
            }

            var cashMovement = new CashMovement
            {
                ShiftId = activeShift.Id,
                Amount = -refundAmount,
                Type = "Salida",
                Reason = reason,
                CreatedAt = DateTime.Now,
                CreatedBy = authorizedBy,
                TenantId = tenantId
            };

            _dbContext.CashMovements.Add(cashMovement);
            activeShift.Movements ??= new List<CashMovement>();
            activeShift.Movements.Add(cashMovement);
        }

        private void AddOutboxMessage(string tenantId, int orderId, string eventType, object payload)
        {
            var outboxMessage = new OutboxMessage
            {
                EventId = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                DeviceId = Environment.MachineName,
                AggregateId = orderId.ToString(),
                EventType = eventType,
                Payload = JsonSerializer.Serialize(payload),
                SchemaVersion = "1.0",
                CreatedAt = DateTime.UtcNow,
                AttemptCount = 0,
                NextAttemptAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _dbContext.OutboxMessages.Add(outboxMessage);
        }
    }
}
