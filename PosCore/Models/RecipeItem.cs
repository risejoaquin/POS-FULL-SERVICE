namespace PosCore.Models;

public class RecipeItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public int SupplyId { get; set; }
    public Supply Supply { get; set; } = null!;
    
    public decimal Quantity { get; set; } = 1;
    
    [System.Text.Json.Serialization.JsonIgnore]
    public string TenantId { get; set; } = string.Empty;
}
