using System.Threading.Tasks;
using PosApplication.DTOs.Local;

namespace PosApplication.Interfaces.Local
{
    public interface IProductLookupService
    {
        Task<ProductLookupResult> LookupByBarcodeAsync(string barcode);
        Task<ProductLookupResult> LookupByIdAsync(int id);
    }
}
