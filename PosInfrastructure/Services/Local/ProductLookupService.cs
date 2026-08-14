using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosDomain.Entities;
using PosInfrastructure.Data.Local;
using PosApplication.Interfaces.Local;
using PosApplication.DTOs.Local;

namespace PosInfrastructure.Services.Local
{
    public class ProductLookupService : IProductLookupService
    {
        private readonly PosDbContext _dbContext;

        public ProductLookupService(PosDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<ProductLookupResult> LookupByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return new ProductLookupResult
                {
                    Found = false,
                    Message = "Código de barras vacío"
                };
            }

            var lowerBarcode = barcode.ToLower().Trim();

            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Barcode.ToLower() == lowerBarcode || p.Name.ToLower() == lowerBarcode);

            if (product != null)
            {
                return new ProductLookupResult
                {
                    Found = true,
                    Product = product,
                    Message = "Producto encontrado"
                };
            }

            return new ProductLookupResult
            {
                Found = false,
                Message = $"Producto no encontrado para el código: {barcode}"
            };
        }

        public async Task<ProductLookupResult> LookupByIdAsync(int id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                return new ProductLookupResult
                {
                    Found = true,
                    Product = product,
                    Message = "Producto encontrado"
                };
            }

            return new ProductLookupResult
            {
                Found = false,
                Message = $"Producto con ID {id} no encontrado"
            };
        }
    }
}
