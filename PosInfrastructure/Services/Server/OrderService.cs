using PosDomain.Interfaces;
using Microsoft.EntityFrameworkCore;
using PosInfrastructure.Data.Server;
using PosDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PosApplication.Interfaces.Server;

namespace PosInfrastructure.Services.Server
{
    public class OrderService : IOrderService
    {
        private readonly CentralDbContext _context;
        private readonly ITenantContext _tenantContext;

        public OrderService(CentralDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<(bool isSuccess, string message, int? orderId)> CreateOrUpdateOrderAsync(Order order)
        {
            var tenantId = _tenantContext.GetTenantId();
            order.TenantId = tenantId;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Validar idempotencia
                    if (!string.IsNullOrEmpty(order.IdempotencyKey) && await _context.Orders.AnyAsync(o => o.IdempotencyKey == order.IdempotencyKey && o.TenantId == tenantId))
                    {
                        var existingOrder = await _context.Orders.Include(o => o.Items).Include(o => o.Payments).FirstOrDefaultAsync(o => o.IdempotencyKey == order.IdempotencyKey && o.TenantId == tenantId);
                        
                        if (existingOrder != null)
                        {
                            // State machine validation for updates
                            bool isValidTransition = false;
                            
                            if (existingOrder.Status == order.Status) {
                                isValidTransition = true;
                            } else if (existingOrder.Status == OrderStatus.Draft && order.Status == OrderStatus.Open) {
                                isValidTransition = true;
                            } else if (existingOrder.Status == OrderStatus.Open && (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Cancelled)) {
                                isValidTransition = true;
                            } else if (existingOrder.Status == OrderStatus.Paid && (order.Status == OrderStatus.Closed || order.Status == OrderStatus.Refunded)) {
                                isValidTransition = true;
                            } else if (existingOrder.Status == OrderStatus.Closed && order.Status == OrderStatus.Refunded) {
                                isValidTransition = true;
                            }

                            if (!isValidTransition) {
                                return (false, $"Transición de estado inválida de {existingOrder.Status} a {order.Status}.", existingOrder.Id);
                            }

                            if (existingOrder.LastUpdated > order.LastUpdated)
                            {
                                return (false, "Conflicto de sincronización: la versión en el servidor es más reciente.", existingOrder.Id);
                            }

                            bool updated = false;

                            if (order.IsReturned && !existingOrder.IsReturned)
                            {
                                // Update as returned
                                existingOrder.IsReturned = true;
                                existingOrder.ReturnReason = order.ReturnReason;
                                existingOrder.AuthorizedBy = order.AuthorizedBy;
                                existingOrder.TotalAmount = order.TotalAmount; // Partial returns change total amount
                                updated = true;

                                // Return inventory
                                foreach (var item in existingOrder.Items)
                                {
                                    var product = await _context.Products
                                        .Include(p => p.RecipeItems)
                                        .ThenInclude(ri => ri.Supply)
                                        .FirstOrDefaultAsync(p => p.Barcode == item.ProductBarcode && p.TenantId == tenantId);
                                    
                                    if (product != null)
                                    {
                                        product.StockQuantity += (int)item.Quantity;
                                        product.LastUpdated = DateTime.UtcNow;

                                        _context.InventoryMovements.Add(new InventoryMovement {
                                            ProductId = product.Id,
                                            Quantity = item.Quantity,
                                            MovementType = "Return",
                                            Reference = existingOrder.IdempotencyKey,
                                            TenantId = tenantId
                                        });

                                        foreach (var recipe in product.RecipeItems)
                                        {
                                            if (recipe.Supply != null)
                                            {
                                                recipe.Supply.Stock += recipe.Quantity * item.Quantity;
                                                
                                                _context.InventoryMovements.Add(new InventoryMovement {
                                                    ProductId = product.Id,
                                                    SupplyId = recipe.SupplyId,
                                                    Quantity = recipe.Quantity * item.Quantity,
                                                    MovementType = "Return",
                                                    Reference = existingOrder.IdempotencyKey,
                                                    TenantId = tenantId
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            
                            if (existingOrder.Status != order.Status) {
                                existingOrder.Status = order.Status;
                                updated = true;
                            }

                            if (updated) {
                                existingOrder.LastUpdated = DateTime.UtcNow;
                                await _context.SaveChangesAsync();
                                await transaction.CommitAsync();
                                return (true, "Orden actualizada exitosamente", existingOrder.Id);
                            }
                        }
                        return (true, "La orden ya había sido registrada anteriormente (Idempotencia).", existingOrder?.Id);
                    }

                    // Validate initial state
                    if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Open && order.Status != OrderStatus.Paid && order.Status != OrderStatus.Closed) 
                    {
                         return (false, $"Estado inicial de orden inválido: {order.Status}. Debe ser Draft, Open, Paid o Closed.", null);
                    }

                    // Enforce Cash Register Shift association
                    var shift = await _context.CashRegisterShifts
                        .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.OpenedAt <= order.OrderDate && (s.ClosedAt == null || s.ClosedAt >= order.OrderDate));

                    if (shift == null)
                    {
                        return (false, "No hay una caja abierta válida en la fecha de la venta para procesar esta transacción.", null);
                    }
                    order.ShiftId = shift.Id;

                    // Resetear el ID de la Orden para evitar conflicto de Clave Primaria en PostgreSQL
                    order.Id = 0;

                    if (order.Items != null && order.Items.Any())
                    {
                        order.Items.RemoveAll(i => i == null);
                        foreach (var item in order.Items)
                        {
                            var product = await _context.Products
                                .Include(p => p.RecipeItems)
                                .ThenInclude(ri => ri.Supply)
                                .FirstOrDefaultAsync(p => p.Barcode == item.ProductBarcode && p.TenantId == tenantId);

                            if (product == null)
                            {
                                return (false, $"El producto con código de barras {item.ProductBarcode} no existe en el catálogo central.", null);
                            }

                            // Deduct Product Stock
                            product.StockQuantity -= (int)item.Quantity;
                            product.LastUpdated = DateTime.UtcNow;

                            _context.InventoryMovements.Add(new InventoryMovement {
                                ProductId = product.Id,
                                Quantity = -item.Quantity,
                                MovementType = "Sale",
                                Reference = order.IdempotencyKey,
                                TenantId = tenantId
                            });

                            // Deduct Supply Stock
                            foreach (var recipe in product.RecipeItems)
                            {
                                if (recipe.Supply != null)
                                {
                                    recipe.Supply.Stock -= recipe.Quantity * item.Quantity;
                                    
                                    _context.InventoryMovements.Add(new InventoryMovement {
                                        ProductId = product.Id,
                                        SupplyId = recipe.SupplyId,
                                        Quantity = -(recipe.Quantity * item.Quantity),
                                        MovementType = "Sale",
                                        Reference = order.IdempotencyKey,
                                        TenantId = tenantId
                                    });
                                }
                            }

                            item.TenantId = tenantId;
                            item.Id = 0;      // Resetear ID del ítem
                            item.OrderId = 0; // Desvincular clave foránea asignada en el SQLite local
                            item.Product = null!; // Avoid detached entity conflicts
                        }
                    }
                    else
                    {
                        order.Items = new List<OrderItem>();
                    }

                    if (order.Payments != null && order.Payments.Any())
                    {
                        order.Payments.RemoveAll(p => p == null);
                        
                        decimal totalApplied = order.Payments.Sum(p => p.Amount);
                        if (Math.Round(totalApplied, 2) != Math.Round(order.TotalAmount, 2))
                        {
                            return (false, $"La suma de pagos ({totalApplied}) no coincide con el total de la orden ({order.TotalAmount}).", null);
                        }

                        foreach (var payment in order.Payments)
                        {
                            payment.TenantId = tenantId;
                            payment.Id = 0;
                            payment.OrderId = 0;
                            payment.Order = null!;
                        }
                    }
                    else
                    {
                        order.Payments = new List<Payment>();
                    }

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return (true, "Orden sincronizada exitosamente", order.Id);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("ERROR CreateOrder: " + ex.ToString());
                    throw new Exception("Error interno al guardar la orden en PostgreSQL", ex);
                }
            });
        }

        public async Task<(List<Order> data, int page, int pageSize, int total)> GetOrdersAsync(int page, int pageSize)
        {
            var tenantId = _tenantContext.GetTenantId();
            
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.TenantId == tenantId)
                .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id);
                
            var total = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.Items)
                .ToListAsync();
                
            return (orders, page, pageSize, total);
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            var tenantId = _tenantContext.GetTenantId();
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId);
        }
    }
}
