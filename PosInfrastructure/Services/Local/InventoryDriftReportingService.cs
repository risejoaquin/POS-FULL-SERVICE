using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosApplication.Interfaces.Local;
using PosDomain.ReadModels;
using PosInfrastructure.Data.Local;

namespace PosInfrastructure.Services.Local
{
    /// <summary>
    /// Read-only integration service that exposes drift detection using current local stock columns
    /// and the inventory ledger read model. This service is diagnostic only and must not mutate stock.
    /// </summary>
    public sealed class InventoryDriftReportingService : IInventoryDriftReportingService
    {
        private readonly PosDbContext _dbContext;

        public InventoryDriftReportingService(PosDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));
        }

        public async Task<InventoryDriftReport> GetProductDriftReportAsync(CancellationToken cancellationToken = default)
        {
            var operationalQuantities = await _dbContext.Products
                .AsNoTracking()
                .Select(product => new
                {
                    product.Id,
                    Quantity = (decimal)product.StockQuantity
                })
                .ToDictionaryAsync(product => product.Id, product => product.Quantity, cancellationToken);

            var movements = await _dbContext.InventoryMovements
                .AsNoTracking()
                .Where(movement => movement.SupplyId == null)
                .ToListAsync(cancellationToken);

            return InventoryDriftDetectionReadModel.DetectProductDrift(movements, operationalQuantities);
        }

        public async Task<InventoryDriftReport> GetSupplyDriftReportAsync(CancellationToken cancellationToken = default)
        {
            var operationalQuantities = await _dbContext.Supplies
                .AsNoTracking()
                .Select(supply => new
                {
                    supply.Id,
                    Quantity = supply.Stock
                })
                .ToDictionaryAsync(supply => supply.Id, supply => supply.Quantity, cancellationToken);

            var movements = await _dbContext.InventoryMovements
                .AsNoTracking()
                .Where(movement => movement.SupplyId.HasValue)
                .ToListAsync(cancellationToken);

            return InventoryDriftDetectionReadModel.DetectSupplyDrift(movements, operationalQuantities);
        }

        public async Task<InventoryDriftReport> GetCombinedDriftReportAsync(CancellationToken cancellationToken = default)
        {
            var productReport = await GetProductDriftReportAsync(cancellationToken);
            var supplyReport = await GetSupplyDriftReportAsync(cancellationToken);

            return new InventoryDriftReport(productReport.Items.Concat(supplyReport.Items));
        }
    }
}
