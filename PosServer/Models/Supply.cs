namespace PosServer.Models;

public class Supply
{
    public int Id { get; set; }
    public string? Name { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; } = "kg";
    public decimal Cost { get; set; } = 0;
    [System.ComponentModel.DataAnnotations.ConcurrencyCheck]
    public decimal Stock { get; set; } = 0;
    public decimal MinStockThreshold { get; set; } = 0;
    
    [System.Text.Json.Serialization.JsonIgnore]
    public string? TenantId { get; set; } = string.Empty;
}
