using PosDomain.Interfaces;
using PosDomain.Entities;
using System.Threading.Tasks;

namespace PosApplication.Interfaces.Server
{
    public interface IInventoryMovementService
    {
        Task<InventoryMovement> SyncMovementAsync(InventoryMovement movement);
    }
}
