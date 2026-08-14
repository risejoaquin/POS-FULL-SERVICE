using PosInfrastructure.Data.Local;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosDomain.Interfaces;
using PosDomain.Entities;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosInfrastructure.Repositories.Local
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(PosDbContext context) : base(context)
        {
        }

        public async Task<Product?> GetByBarcodeAsync(string barcode, string tenantId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Barcode == barcode && p.TenantId == tenantId);
        }
    }
}
