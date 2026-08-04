using System.Collections.Generic;

namespace PosCore.Models
{
    public class SyncPayload
    {
        public List<Product> Products { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<CashRegisterShift> Shifts { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
    }
}
