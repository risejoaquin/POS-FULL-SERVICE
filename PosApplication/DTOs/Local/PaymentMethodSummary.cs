namespace PosApplication.DTOs.Local;

public class PaymentMethodSummary
{
    public string Method { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public double ChartHeight { get; set; }
}
