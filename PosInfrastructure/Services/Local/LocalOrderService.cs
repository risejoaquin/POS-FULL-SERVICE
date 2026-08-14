using System;
using System.Collections.Generic;
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
    public class LocalOrderService : ILocalOrderService
    {
        private readonly PosDbContext _dbContext;
        private readonly IInventoryService _inventoryService;

        public LocalOrderService(PosDbContext dbContext, IInventoryService inventoryService)
        {
            _dbContext = dbContext;
            _inventoryService = inventoryService;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new InvalidOperationException($"Orden {id} no encontrada.");
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                .Where(o => o.Status == OrderStatus.Draft || o.Status == OrderStatus.Open)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();
            return order;
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Order> SaveOrderAsync(Order order)
        {
            if (order.Id == 0)
            {
                _dbContext.Orders.Add(order);
            }
            else
            {
                _dbContext.Orders.Update(order);
            }

            await _dbContext.SaveChangesAsync();
            return order;
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _dbContext.Orders.FindAsync(id);
            if (order == null) return;

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> HasActiveShiftAsync(string tenantId)
        {
            return await _dbContext.CashRegisterShifts
                .AnyAsync(s => s.TenantId == tenantId && !s.IsClosed);
        }

        public async Task<CheckoutResult> ProcessCheckoutAsync(CheckoutRequest request)
        {
            if (request.Lines == null || request.Lines.Count == 0)
            {
                return CheckoutResult.Fail("La venta no contiene productos.", request.ChangeDue, request.IdempotencyKey);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var activeShift = await _dbContext.CashRegisterShifts
                    .Where(s => s.TenantId == request.TenantId && !s.IsClosed)
                    .OrderByDescending(s => s.OpenedAt)
                    .FirstOrDefaultAsync();

                if (activeShift == null)
                {
                    throw new InvalidOperationException("No hay un turno abierto. Por favor, abra un turno.");
                }

                var productsToUpdate = new List<(CheckoutLineRequest line, Product product)>();
                foreach (var line in request.Lines)
                {
                    var product = await _dbContext.Products
                        .Include(p => p.RecipeItems)
                            .ThenInclude(r => r.Supply)
                        .FirstOrDefaultAsync(p => p.Id == line.ProductId);

                    if (product == null)
                    {
                        throw new InvalidOperationException($"Producto no encontrado: {line.ProductName}");
                    }

                    var canFulfill = product.CanFulfill(line.Quantity);
                    if (!canFulfill.IsSuccess)
                    {
                        throw new InvalidOperationException($"Stock insuficiente para {product.Name}.");
                    }

                    productsToUpdate.Add((line, product));
                }

                foreach (var update in productsToUpdate)
                {
                    var line = update.line;
                    var product = update.product;

                    var decreaseProductStock = product.DecreaseStock(line.Quantity);
                    if (!decreaseProductStock.IsSuccess)
                    {
                        throw new InvalidOperationException(decreaseProductStock.Error);
                    }

                    _dbContext.InventoryMovements.Add(new InventoryMovement
                    {
                        ProductId = product.Id,
                        Quantity = -line.Quantity,
                        MovementType = "Sale",
                        Reference = "Venta de producto",
                        TenantId = request.TenantId,
                        MovementDate = DateTime.Now
                    });

                    if (product.RecipeItems != null && product.RecipeItems.Any())
                    {
                        foreach (var recipeItem in product.RecipeItems)
                        {
                            if (recipeItem.Supply == null) continue;

                            var deducted = recipeItem.RequiredFor(line.Quantity);
                            var decreaseSupplyStock = recipeItem.Supply.DecreaseStock(deducted);
                            if (!decreaseSupplyStock.IsSuccess)
                            {
                                throw new InvalidOperationException($"Stock insuficiente para el insumo {recipeItem.Supply.Name}.");
                            }

                            _dbContext.Supplies.Update(recipeItem.Supply);

                            _dbContext.InventoryMovements.Add(new InventoryMovement
                            {
                                ProductId = product.Id,
                                SupplyId = recipeItem.SupplyId,
                                Quantity = -deducted,
                                MovementType = "Sale",
                                Reference = $"Consumo por venta de producto {product.Name}",
                                TenantId = request.TenantId,
                                MovementDate = DateTime.Now
                            });
                        }
                    }
                }

                var paymentDetails = BuildPaymentDetails(request);
                var payments = request.Payments.Select(p => new Payment
                {
                    Amount = p.AppliedAmount,
                    Method = p.Method,
                    PaymentDate = DateTime.Now,
                    ShiftId = activeShift.Id,
                    IdempotencyKey = request.IdempotencyKey,
                    TenantId = request.TenantId
                }).ToList();

                var totalApplied = payments.Sum(p => p.Amount);
                if (Math.Round(totalApplied, 2) != Math.Round(request.TotalAmount, 2))
                {
                    throw new InvalidOperationException($"La suma de pagos ({totalApplied}) no coincide con el total de la orden ({request.TotalAmount}).");
                }

                var order = new Order
                {
                    Status = OrderStatus.Draft,
                    OrderDate = request.OrderDate,
                    CustomerName = request.CustomerName,
                    SubTotal = request.SubTotal,
                    TaxAmount = request.TaxAmount,
                    TotalAmount = request.TotalAmount,
                    Items = request.Lines.Select(line => new OrderItem
                    {
                        ProductId = line.ProductId,
                        ProductBarcode = line.ProductBarcode,
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice,
                        Discount = line.Discount,
                        Notes = line.Notes,
                        TenantId = request.TenantId
                    }).ToList(),
                    Payments = payments,
                    IsReturned = false,
                    PaymentDetails = paymentDetails,
                    CreatedById = request.CreatedById,
                    TenantId = request.TenantId,
                    IdempotencyKey = request.IdempotencyKey,
                    ShiftId = activeShift.Id
                };

                var orderManager = new OrderManagementService(_dbContext, _inventoryService);
                _dbContext.Orders.Add(order);
                orderManager.TransitionState(order, OrderStatus.Open);
                orderManager.TransitionState(order, OrderStatus.Paid);

                _dbContext.CashMovements.Add(new CashMovement
                {
                    ShiftId = activeShift.Id,
                    Type = "Entrada",
                    Amount = request.TotalAmount,
                    Reason = "Venta de orden (Pago total)",
                    CreatedBy = request.CreatedById,
                    CreatedAt = DateTime.Now,
                    TenantId = request.TenantId
                });

                orderManager.TransitionState(order, OrderStatus.Closed);

                var retries = 3;
                while (true)
                {
                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        break;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        retries--;
                        if (retries == 0) throw;
                        await ResolveCheckoutConcurrencyAsync(ex, request);
                    }
                }

                return CheckoutResult.Success(order.Id, request.ChangeDue, request.IdempotencyKey, paymentDetails);
            }
            catch
            {
                await transaction.RollbackAsync();
                _dbContext.ChangeTracker.Clear();
                throw;
            }
        }

        private static string BuildPaymentDetails(CheckoutRequest request)
        {
            var paymentDetailsList = request.Payments
                .Select(p => $"{p.Method}: {p.AppliedAmount:C}")
                .ToList();

            var paymentDetails = string.Join(", ", paymentDetailsList);
            if (request.ChangeDue > 0)
            {
                paymentDetails += $" (Cambio: {request.ChangeDue:C})";
            }

            return paymentDetails;
        }

        private static async Task ResolveCheckoutConcurrencyAsync(DbUpdateConcurrencyException ex, CheckoutRequest request)
        {
            foreach (var entry in ex.Entries)
            {
                var databaseValues = await entry.GetDatabaseValuesAsync();
                if (databaseValues == null) continue;

                var originalSupplyStock = 0m;
                if (entry.Entity is Supply)
                {
                    originalSupplyStock = (decimal)(entry.OriginalValues["Stock"] ?? 0m);
                }

                entry.OriginalValues.SetValues(databaseValues);

                if (entry.Entity is Product p)
                {
                    var dbStock = (int)(databaseValues["StockQuantity"] ?? 0);
                    var line = request.Lines.FirstOrDefault(i => i.ProductId == p.Id);
                    if (line != null)
                    {
                        var recalculatedStock = dbStock - line.Quantity;
                        if (recalculatedStock < 0)
                        {
                            throw new InvalidOperationException($"Stock insuficiente para {p.Name} después de resolver concurrencia.");
                        }

                        entry.CurrentValues["StockQuantity"] = recalculatedStock;
                    }
                }
                else if (entry.Entity is Supply)
                {
                    var dbStock = (decimal)(databaseValues["Stock"] ?? 0m);
                    var deducted = originalSupplyStock - (decimal)(entry.CurrentValues["Stock"] ?? 0m);
                    var recalculatedStock = dbStock - deducted;
                    if (recalculatedStock < 0)
                    {
                        throw new InvalidOperationException("Stock insuficiente para el insumo después de resolver concurrencia.");
                    }

                    entry.CurrentValues["Stock"] = recalculatedStock;
                }
            }
        }
    }
}
