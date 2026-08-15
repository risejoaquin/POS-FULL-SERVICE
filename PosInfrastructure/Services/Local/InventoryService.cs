using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosDomain.Entities;
using PosInfrastructure.Data.Local;
using PosApplication.Interfaces.Local;

namespace PosInfrastructure.Services.Local
{
    public class InventoryService : IInventoryService
    {
        private readonly PosDbContext _dbContext;

        public InventoryService(PosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task RegisterSaleAsync(int productId, decimal quantity, string reference, string tenantId)
        {
            var product = await _dbContext.Products
                .Include(p => p.RecipeItems)
                .ThenInclude(ri => ri.Supply)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return;

            var productQuantity = ToWholeProductQuantity(quantity);
            var decreaseProductStock = product.DecreaseStock(productQuantity);
            if (!decreaseProductStock.IsSuccess)
            {
                throw new InvalidOperationException($"Stock insuficiente para {product.Name}");
            }

            var movement = new InventoryMovement
            {
                ProductId = productId,
                Quantity = quantity,
                MovementType = InventoryMovement.SaleType,
                MovementDate = DateTime.UtcNow,
                Reference = reference,
                TenantId = tenantId
            };
            _dbContext.InventoryMovements.Add(movement);

            if (product.RecipeItems != null && product.RecipeItems.Any())
            {
                foreach (var recipeItem in product.RecipeItems)
                {
                    if (recipeItem.Supply != null)
                    {
                        var requiredSupplyQuantity = recipeItem.RequiredFor(productQuantity);
                        var decreaseSupplyStock = recipeItem.Supply.DecreaseStock(requiredSupplyQuantity);
                        if (!decreaseSupplyStock.IsSuccess)
                        {
                            throw new InvalidOperationException($"Stock insuficiente para el insumo {recipeItem.Supply.Name}");
                        }

                        _dbContext.Supplies.Update(recipeItem.Supply);
                        
                        var supplyMovement = new InventoryMovement
                        {
                            SupplyId = recipeItem.Supply.Id,
                            Quantity = requiredSupplyQuantity,
                            MovementType = InventoryMovement.RecipeConsumptionType,
                            MovementDate = DateTime.UtcNow,
                            Reference = reference,
                            TenantId = tenantId
                        };
                        _dbContext.InventoryMovements.Add(supplyMovement);
                    }
                }
            }
            
            _dbContext.Products.Update(product);
        }

        public async Task RegisterReturnAsync(int productId, decimal quantity, string reference, string tenantId)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return;

            var increaseProductStock = product.IncreaseStock(ToWholeProductQuantity(quantity));
            if (!increaseProductStock.IsSuccess)
            {
                throw new InvalidOperationException(increaseProductStock.Error);
            }

            var movement = new InventoryMovement
            {
                ProductId = productId,
                Quantity = quantity,
                MovementType = InventoryMovement.ReturnType,
                MovementDate = DateTime.UtcNow,
                Reference = reference,
                TenantId = tenantId
            };
            _dbContext.InventoryMovements.Add(movement);
            _dbContext.Products.Update(product);
        }

        public async Task RegisterRestockAsync(int productId, decimal quantity, string reference, string tenantId)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return;

            var increaseProductStock = product.IncreaseStock(ToWholeProductQuantity(quantity));
            if (!increaseProductStock.IsSuccess)
            {
                throw new InvalidOperationException(increaseProductStock.Error);
            }

            var movement = new InventoryMovement
            {
                ProductId = productId,
                Quantity = quantity,
                MovementType = InventoryMovement.RestockType,
                MovementDate = DateTime.UtcNow,
                Reference = reference,
                TenantId = tenantId
            };
            _dbContext.InventoryMovements.Add(movement);
            _dbContext.Products.Update(product);
        }

        private static int ToWholeProductQuantity(decimal quantity)
        {
            if (quantity <= 0 || quantity != decimal.Truncate(quantity))
            {
                throw new InvalidOperationException("La cantidad de producto debe ser un entero mayor a cero.");
            }

            return (int)quantity;
        }
    }
}
