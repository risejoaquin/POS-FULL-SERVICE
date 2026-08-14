using PosDomain;
using System.Threading.Tasks;
using PosDomain.Entities;

namespace PosDomain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByBarcodeAsync(string barcode, string tenantId);
    }
}
