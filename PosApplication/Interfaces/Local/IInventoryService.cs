using System.Threading.Tasks;

namespace PosApplication.Interfaces.Local
{
    public interface IInventoryService
    {
        Task RegisterSaleAsync(int productId, decimal quantity, string reference, string tenantId);
        Task RegisterReturnAsync(int productId, decimal quantity, string reference, string tenantId);
        Task RegisterRestockAsync(int productId, decimal quantity, string reference, string tenantId);
    }
}
