using System.Collections.Generic;
namespace PosDomain.Entities
{
    public class SyncPayload
    {
        public List<Product> Products { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<CashRegisterShift> Shifts { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<Supply> Supplies { get; set; } = new();
        public List<ProductModifier> ProductModifiers { get; set; } = new();
    }
}