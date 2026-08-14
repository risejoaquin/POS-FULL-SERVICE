using System.Collections.Generic;
using System;
namespace PosServer.Models;
public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; } = string.Empty;
    public string? Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    [System.ComponentModel.DataAnnotations.ConcurrencyCheck]
    public int StockQuantity { get; set; }
    public int MinStockThreshold { get; set; } = 10;
    public string? Category { get; set; } = "General";
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string? TenantId { get; set; } = string.Empty;
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
    public List<RecipeItem> RecipeItems { get; set; } = new();
    public ICollection<ProductModifierLink> ProductModifiers { get; set; } = new List<ProductModifierLink>();
}
