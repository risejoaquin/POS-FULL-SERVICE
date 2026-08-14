using System.Collections.Generic;
namespace PosDomain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public int ProductId { get; set; }
    public string ProductBarcode { get; set; } = string.Empty;
    public Product Product { get; set; } = null!;
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Propiedad calculada
    public decimal Discount { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;
    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    public bool HasDiscount => Discount > 0;
    public decimal SubTotal => (Quantity * UnitPrice) - Discount;
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public string TenantId { get; set; } = string.Empty;
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
}
