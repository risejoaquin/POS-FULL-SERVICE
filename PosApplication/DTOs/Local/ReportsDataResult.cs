using PosDomain.Entities;

namespace PosApplication.DTOs.Local;

public class ReportsDataResult
{
    public List<DailySalesSummary> DailySales { get; set; } = new();
    public List<ProductSaleSummary> TopProducts { get; set; } = new();
    public List<Product> LowStockProducts { get; set; } = new();
    public List<Supply> LowStockSupplies { get; set; } = new();
    public List<PaymentMethodSummary> PaymentMethods { get; set; } = new();
    public List<CashRegisterShift> ShiftHistory { get; set; } = new();
    public List<CashMovement> CashMovementRows { get; set; } = new();
    public decimal PeriodTotalRevenue { get; set; }
    public int PeriodTotalOrders { get; set; }
}
