using System.Threading;
using System.Threading.Tasks;
using PosDomain.ReadModels;

namespace PosApplication.Interfaces.Local
{
    /// <summary>
    /// Provides read-only inventory drift diagnostics for local inventory.
    /// This service must not correct or mutate stock.
    /// </summary>
    public interface IInventoryDriftReportingService
    {
        Task<InventoryDriftReport> GetProductDriftReportAsync(CancellationToken cancellationToken = default);
        Task<InventoryDriftReport> GetSupplyDriftReportAsync(CancellationToken cancellationToken = default);
        Task<InventoryDriftReport> GetCombinedDriftReportAsync(CancellationToken cancellationToken = default);
    }
}
