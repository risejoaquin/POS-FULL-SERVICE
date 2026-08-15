using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosApplication.Interfaces.Local;
using PosDomain.Entities;
using PosInfrastructure.Data.Local;

namespace PosInfrastructure.Services.Local
{
    public class InventoryAppService : IInventoryAppService
    {
        private readonly PosDbContext _dbContext;

        public InventoryAppService(PosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _dbContext.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Supply>> GetAllSuppliesAsync()
        {
            return await _dbContext.Supplies
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            var barcodeExists = await _dbContext.Products.AnyAsync(p => p.Barcode == product.Barcode);
            if (barcodeExists)
            {
                throw new InvalidOperationException("El código de barras ya existe.");
            }

            product.LastUpdated = DateTime.UtcNow;
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            if (product.StockQuantity > 0)
            {
                _dbContext.InventoryMovements.Add(InventoryMovement.ProductRestock(
                    product.Id,
                    product.StockQuantity,
                    product.TenantId,
                    "Stock inicial"));
                await _dbContext.SaveChangesAsync();
            }

            return product;
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            var barcodeExists = await _dbContext.Products.AnyAsync(p => p.Barcode == product.Barcode && p.Id != product.Id);
            if (barcodeExists)
            {
                throw new InvalidOperationException("El código de barras ya está asignado a otro producto.");
            }

            var existing = await _dbContext.Products.FindAsync(product.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Producto no encontrado.");
            }

            existing.Name = product.Name;
            existing.Barcode = product.Barcode;
            existing.Price = product.Price;
            existing.MinStockThreshold = product.MinStockThreshold;
            existing.Category = product.Category;
            existing.ImagePath = product.ImagePath;
            existing.CustomAttributes = product.CustomAttributes;
            existing.LastUpdated = DateTime.UtcNow;

            var stockAdjustment = product.StockQuantity - existing.StockQuantity;
            if (stockAdjustment != 0)
            {
                ApplyProductStockAdjustment(existing, stockAdjustment);
                _dbContext.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = existing.Id,
                    Quantity = stockAdjustment,
                    MovementType = InventoryMovement.AdjustmentType,
                    MovementDate = DateTime.UtcNow,
                    Reference = "Ajuste por edición de producto",
                    TenantId = existing.TenantId
                });
            }

            _dbContext.Products.Update(existing);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int productId)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null) return;

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<int> ImportProductsAsync(IEnumerable<Product> products)
        {
            if (products == null) throw new ArgumentNullException(nameof(products));

            var importedProducts = new List<Product>();
            foreach (var product in products)
            {
                if (product == null) continue;
                if (string.IsNullOrWhiteSpace(product.Barcode) || string.IsNullOrWhiteSpace(product.Name)) continue;

                var barcodeExists = await _dbContext.Products.AnyAsync(p => p.Barcode == product.Barcode);
                if (barcodeExists) continue;

                product.LastUpdated = DateTime.UtcNow;
                _dbContext.Products.Add(product);
                importedProducts.Add(product);
            }

            if (importedProducts.Count > 0)
            {
                await _dbContext.SaveChangesAsync();

                foreach (var importedProduct in importedProducts.Where(p => p.StockQuantity > 0))
                {
                    _dbContext.InventoryMovements.Add(InventoryMovement.ProductRestock(
                        importedProduct.Id,
                        importedProduct.StockQuantity,
                        importedProduct.TenantId,
                        "Stock inicial importado"));
                }

                await _dbContext.SaveChangesAsync();
            }

            return importedProducts.Count;
        }

        public async Task<IEnumerable<RecipeItem>> GetProductRecipeItemsAsync(int productId)
        {
            return await _dbContext.RecipeItems
                .AsNoTracking()
                .Include(r => r.Supply)
                .Where(r => r.ProductId == productId)
                .OrderBy(r => r.Supply.Name)
                .ToListAsync();
        }

        public async Task<RecipeItem> AddRecipeItemAsync(int productId, int supplyId, decimal quantity)
        {
            if (quantity <= 0) throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            var productExists = await _dbContext.Products.AnyAsync(p => p.Id == productId);
            if (!productExists) throw new InvalidOperationException("Producto no encontrado.");

            var supply = await _dbContext.Supplies.FindAsync(supplyId);
            if (supply == null) throw new InvalidOperationException("Insumo no encontrado.");

            var exists = await _dbContext.RecipeItems.AnyAsync(r => r.ProductId == productId && r.SupplyId == supplyId);
            if (exists) throw new InvalidOperationException("Este insumo ya está en la receta.");

            var item = new RecipeItem
            {
                ProductId = productId,
                SupplyId = supplyId,
                Quantity = quantity,
                Supply = supply
            };

            _dbContext.RecipeItems.Add(item);
            await _dbContext.SaveChangesAsync();
            return item;
        }

        public async Task DeleteRecipeItemAsync(int recipeItemId)
        {
            var item = await _dbContext.RecipeItems.FindAsync(recipeItemId);
            if (item == null) return;

            _dbContext.RecipeItems.Remove(item);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductModifier>> GetAllProductModifiersAsync()
        {
            return await _dbContext.ProductModifiers
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifier>> GetProductModifiersAsync(int productId)
        {
            return await _dbContext.ProductModifierLinks
                .AsNoTracking()
                .Include(l => l.ProductModifier)
                .Where(l => l.ProductId == productId)
                .OrderBy(l => l.SortOrder)
                .Select(l => l.ProductModifier)
                .ToListAsync();
        }

        public async Task UpdateProductModifiersAsync(int productId, IEnumerable<int> modifierIds)
        {
            var productExists = await _dbContext.Products.AnyAsync(p => p.Id == productId);
            if (!productExists) throw new InvalidOperationException("Producto no encontrado.");

            var existingLinks = await _dbContext.ProductModifierLinks
                .Where(l => l.ProductId == productId)
                .ToListAsync();
            _dbContext.ProductModifierLinks.RemoveRange(existingLinks);

            var orderedIds = modifierIds?.Distinct().ToList() ?? new List<int>();
            for (var i = 0; i < orderedIds.Count; i++)
            {
                _dbContext.ProductModifierLinks.Add(new ProductModifierLink
                {
                    ProductId = productId,
                    ProductModifierId = orderedIds[i],
                    SortOrder = i
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<Supply> CreateSupplyAsync(Supply supply)
        {
            if (supply == null) throw new ArgumentNullException(nameof(supply));

            _dbContext.Supplies.Add(supply);
            await _dbContext.SaveChangesAsync();
            return supply;
        }

        public async Task UpdateSupplyAsync(Supply supply)
        {
            if (supply == null) throw new ArgumentNullException(nameof(supply));

            var existing = await _dbContext.Supplies.FindAsync(supply.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Insumo no encontrado.");
            }

            existing.Name = supply.Name;
            existing.UnitOfMeasure = supply.UnitOfMeasure;
            existing.Cost = supply.Cost;
            existing.Stock = supply.Stock;
            existing.MinStockThreshold = supply.MinStockThreshold;
            existing.TenantId = supply.TenantId;

            _dbContext.Supplies.Update(existing);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteSupplyAsync(int supplyId)
        {
            var supply = await _dbContext.Supplies.FindAsync(supplyId);
            if (supply == null) return;

            _dbContext.Supplies.Remove(supply);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AdjustStockAsync(int productId, decimal quantity, string reason)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null)
            {
                throw new InvalidOperationException("Producto no encontrado.");
            }

            ApplyProductStockAdjustment(product, quantity);

            _dbContext.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = productId,
                Quantity = quantity,
                MovementType = InventoryMovement.AdjustmentType,
                MovementDate = DateTime.UtcNow,
                Reference = reason ?? string.Empty,
                TenantId = product.TenantId
            });

            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }

        private static void ApplyProductStockAdjustment(Product product, decimal quantity)
        {
            if (quantity == 0 || quantity != decimal.Truncate(quantity))
            {
                throw new InvalidOperationException("La cantidad de ajuste debe ser un entero distinto de cero.");
            }

            var result = quantity > 0
                ? product.IncreaseStock((int)quantity)
                : product.DecreaseStock((int)Math.Abs(quantity));

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error);
            }
        }

        public async Task UpdateProductRecipeAsync(int productId, List<RecipeItem> recipeItems)
        {
            var product = await _dbContext.Products
                .Include(p => p.RecipeItems)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                throw new InvalidOperationException("Producto no encontrado.");
            }

            _dbContext.RecipeItems.RemoveRange(product.RecipeItems);
            if (recipeItems != null)
            {
                foreach (var item in recipeItems)
                {
                    item.ProductId = productId;
                    _dbContext.RecipeItems.Add(item);
                }
            }

            product.LastUpdated = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}
