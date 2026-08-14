using Microsoft.EntityFrameworkCore;
using PosApplication.DTOs.Local;
using PosApplication.Interfaces.Local;
using PosInfrastructure.Data.Local;

namespace PosInfrastructure.Services.Local;

public class ReportsService : IReportsService
{
    private readonly PosDbContext _dbContext;

    public ReportsService(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReportsDataResult> GetReportsDataAsync(DateTime startDate, DateTime endDate)
    {
        var actualEndDate = endDate.Date.AddDays(1).AddTicks(-1);

        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.OrderDate >= startDate.Date && o.OrderDate <= actualEndDate)
            .ToListAsync();

        var validOrders = orders.Where(o => !o.IsReturned).ToList();
        var result = new ReportsDataResult
        {
            PeriodTotalRevenue = validOrders.Sum(o => o.TotalAmount),
            PeriodTotalOrders = validOrders.Count
        };

        var salesByDay = validOrders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new DailySalesSummary
            {
                Date = g.Key,
                TotalOrders = g.Count(),
                TotalRevenue = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(d => d.Date)
            .ToList();

        var maxRevenue = salesByDay.Any() ? salesByDay.Max(s => s.TotalRevenue) : 1;
        if (maxRevenue == 0) maxRevenue = 1;

        foreach (var s in salesByDay)
        {
            s.ChartHeight = (double)(s.TotalRevenue / maxRevenue) * 120.0;
            if (s.ChartHeight < 5) s.ChartHeight = 5;
            result.DailySales.Add(s);
        }

        var allItems = validOrders.SelectMany(o => o.Items).ToList();
        result.TopProducts = allItems
            .GroupBy(i => i.ProductId)
            .Select(g => new ProductSaleSummary
            {
                ProductName = g.First().Product?.Name ?? g.First().ProductBarcode,
                QuantitySold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.SubTotal)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(20)
            .ToList();

        result.PaymentMethods = validOrders
            .SelectMany(o => (o.PaymentDetails ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .GroupBy(p =>
            {
                var parts = p.Split(':');
                return parts[0].Trim();
            })
            .Select(g => new PaymentMethodSummary
            {
                Method = g.Key,
                TransactionCount = g.Count(),
                TotalRevenue = g.Sum(p =>
                {
                    var parts = p.Split(':');
                    if (parts.Length > 1 && decimal.TryParse(parts[1].Trim().TrimStart('$'), System.Globalization.NumberStyles.Any, null, out decimal amount))
                    {
                        return amount;
                    }

                    return 0m;
                })
            })
            .OrderByDescending(p => p.TotalRevenue)
            .ToList();

        result.ShiftHistory = await _dbContext.CashRegisterShifts
            .Where(s => s.OpenedAt >= startDate.Date && s.OpenedAt <= actualEndDate)
            .OrderByDescending(s => s.OpenedAt)
            .ToListAsync();

        result.CashMovementRows = await _dbContext.CashMovements
            .Where(m => m.CreatedAt >= startDate.Date && m.CreatedAt <= actualEndDate)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        result.LowStockProducts = await _dbContext.Products
            .Where(p => p.StockQuantity <= p.MinStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();

        result.LowStockSupplies = await _dbContext.Supplies
            .Where(s => s.Stock <= s.MinStockThreshold)
            .OrderBy(s => s.Stock)
            .ToListAsync();

        return result;
    }
}
