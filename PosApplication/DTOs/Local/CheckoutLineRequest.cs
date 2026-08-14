namespace PosApplication.DTOs.Local
{
    public class CheckoutLineRequest
    {
        public int ProductId { get; set; }
        public string ProductBarcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
    }
}
