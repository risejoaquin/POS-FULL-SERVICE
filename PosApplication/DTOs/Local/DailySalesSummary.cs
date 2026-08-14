namespace PosApplication.DTOs.Local;

public class DailySalesSummary
{
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public double ChartHeight { get; set; }
}
