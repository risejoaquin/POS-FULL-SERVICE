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
    public class ProductService : IProductService
    {
        private readonly CentralDbContext _context;
        private readonly ITenantContext _tenantContext;

        public ProductService(CentralDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<(List<Product> data, int page, int pageSize, int total)> GetProductsAsync(int page, int pageSize)
        {
            var tenantId = _tenantContext.GetTenantId();
            var query = _context.Products.Where(p => p.TenantId == tenantId);
            var total = await query.CountAsync();
            var products = await query
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (products, page, pageSize, total);
        }

        public async Task<List<Product>> GetChangesAsync(DateTime since)
        {
            var tenantId = _tenantContext.GetTenantId();
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.TenantId == tenantId && p.LastUpdated > since)
                .ToListAsync();
        }

        public async Task<(bool isSuccess, string message, Product product)> CreateOrUpdateProductAsync(Product product)
        {
            var tenantId = _tenantContext.GetTenantId();
            product.TenantId = tenantId;

            var existing = await _context.Products
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == product.Barcode);

            if (existing == null)
            {
                product.Id = 0; // Garantizar ID autonumerado en PostgreSQL
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (product.StockQuantity > 0)
                {
                    _context.InventoryMovements.Add(InventoryMovement.ProductRestock(
                        product.Id,
                        product.StockQuantity,
                        tenantId,
                        "Stock inicial"));
                    await _context.SaveChangesAsync();
                }

                return (true, "Success", product);
            }
            else
            {
                // Estrategia de Resolución de Conflictos (Last Write Wins)
                if (existing.LastUpdated > product.LastUpdated)
                {
                    return (false, "Conflicto de sincronización: la versión en el servidor es más reciente.", existing);
                }

                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.MinStockThreshold = product.MinStockThreshold;
                existing.Category = product.Category;
                existing.CustomAttributes = product.CustomAttributes;

                var stockAdjustment = product.StockQuantity - existing.StockQuantity;
                if (stockAdjustment != 0)
                {
                    var stockResult = stockAdjustment > 0
                        ? existing.IncreaseStock(stockAdjustment)
                        : existing.DecreaseStock(Math.Abs(stockAdjustment));

                    if (!stockResult.IsSuccess)
                    {
                        return (false, stockResult.Error, existing);
                    }

                    _context.InventoryMovements.Add(new InventoryMovement
                    {
                        ProductId = existing.Id,
                        Quantity = stockAdjustment,
                        MovementType = InventoryMovement.AdjustmentType,
                        MovementDate = DateTime.UtcNow,
                        Reference = "Ajuste por actualización de producto",
                        TenantId = tenantId
                    });
                }

                existing.LastUpdated = DateTime.UtcNow; // Actualizar con hora del servidor
            }

            await _context.SaveChangesAsync();
            return (true, "Success", existing);
        }

        public async Task<bool> DeleteProductAsync(string barcode)
        {
            var tenantId = _tenantContext.GetTenantId();
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == barcode);
            if (existing != null)
            {
                _context.Products.Remove(existing);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
