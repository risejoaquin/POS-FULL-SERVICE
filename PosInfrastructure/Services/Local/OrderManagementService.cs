using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosInfrastructure.Data.Local;
using PosDomain.Entities;
using PosApplication.Interfaces.Local;

namespace PosInfrastructure.Services.Local
{
    public class OrderManagementService
    {
        private readonly PosDbContext _dbContext;
        private readonly IInventoryService _inventoryService;

        public OrderManagementService(PosDbContext dbContext, IInventoryService inventoryService)
        {
            _dbContext = dbContext;
            _inventoryService = inventoryService;
        }

        public void TransitionState(Order order, OrderStatus nextState)
        {
            if (!IsValidTransition(order.Status, nextState))
            {
                throw new InvalidOperationException($"Transición de estado inválida: {order.Status} -> {nextState}");
            }
            order.Status = nextState;
        }

        private bool IsValidTransition(OrderStatus current, OrderStatus next)
        {
            if (current == next) return true;

            return current switch
            {
                OrderStatus.Draft => next == OrderStatus.Open,
                OrderStatus.Open => next == OrderStatus.Paid || next == OrderStatus.Cancelled,
                OrderStatus.Paid => next == OrderStatus.Closed || next == OrderStatus.Refunded,
                OrderStatus.Closed => next == OrderStatus.Refunded,
                _ => false
            };
        }

        public async Task ProcessCheckoutAsync(
            string customerName,
            decimal totalAmount,
            List<OrderItem> items,
            string paymentDetails,
            string createdById,
            string tenantId,
            decimal taxRate = 0.16m)
        {
            // The entire flow encapsulated in a single ACID transaction
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            
            try
            {
                var currentShift = await _dbContext.CashRegisterShifts
                    .Where(s => s.TenantId == tenantId && !s.IsClosed)
                    .OrderByDescending(s => s.OpenedAt)
                    .FirstOrDefaultAsync();
                
                if (currentShift == null)
                {
                    throw new InvalidOperationException("No hay una caja abierta válida para procesar esta transacción.");
                }

                // 1. Order Creation (Draft)
                decimal subtotal = totalAmount / (1 + taxRate);
                decimal taxes = totalAmount - subtotal;
                
                var order = new Order
                {
                    Status = OrderStatus.Draft,
                    OrderDate = DateTime.Now,
                    CustomerName = customerName,
                    SubTotal = subtotal,
                    TaxAmount = taxes,
                    TotalAmount = totalAmount,
                    PaymentDetails = paymentDetails,
                    CreatedById = createdById,
                    TenantId = tenantId,
                    IsReturned = false,
                    ShiftId = currentShift.Id
                };

                _dbContext.Orders.Add(order);
                
                // Draft -> Open
                TransitionState(order, OrderStatus.Open);
                
                // We need to save changes to get the Order ID if we are relying on it for reference? Wait, the outbox message does.
                // In SQLite, if ID is auto-generated, we must save changes to get it.
                await _dbContext.SaveChangesAsync();

                // 2. Insert items and Calculate Totals
                foreach (var item in items)
                {
                    item.Product = null!; // Avoid detached entity conflicts
                    item.OrderId = order.Id;
                    order.Items.Add(item);

                    // 5. Adjust Inventory through InventoryService
                    await _inventoryService.RegisterSaleAsync(item.ProductId, item.Quantity, order.Id.ToString(), tenantId);
                }

                // 4. Payment Registration (Paid)
                TransitionState(order, OrderStatus.Paid);

                // 6. Cash movement registration (if paid in cash and shift is open)
                if (currentShift != null)
                {
                    var cashMovement = new CashMovement
                    {
                        ShiftId = currentShift.Id,
                        Type = "Entrada",
                        Amount = totalAmount,
                        Reason = $"Venta de Orden {order.Id}",
                        CreatedBy = createdById,
                        CreatedAt = DateTime.Now,
                        TenantId = tenantId
                    };
                    _dbContext.CashMovements.Add(cashMovement);
                }

                // 7. Change state to CLOSED
                TransitionState(order, OrderStatus.Closed);
                
                // 8. Create Outbox Message
                var outboxMessage = new OutboxMessage
                {
                    EventId = Guid.NewGuid().ToString(),
                    TenantId = tenantId,
                    DeviceId = Environment.MachineName, // Simple fallback
                    AggregateId = order.Id.ToString(),
                    EventType = "OrderCompleted",
                    Payload = System.Text.Json.JsonSerializer.Serialize(new { OrderId = order.Id, TotalAmount = order.TotalAmount }),
                    SchemaVersion = "1.0",
                    CreatedAt = DateTime.UtcNow,
                    AttemptCount = 0,
                    NextAttemptAt = DateTime.UtcNow,
                    Status = "Pending"
                };
                _dbContext.OutboxMessages.Add(outboxMessage);

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Error de concurrencia al procesar el inventario. Es posible que el stock haya cambiado en otra terminal.", ex);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Stock negativo detectado o error de base de datos.", ex);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
