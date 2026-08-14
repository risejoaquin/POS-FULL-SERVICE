namespace PosApplication.DTOs.Local;

public class ProductSaleSummary
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public double ChartHeight { get; set; }
}
