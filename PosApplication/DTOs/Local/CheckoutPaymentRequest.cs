namespace PosApplication.DTOs.Local
{
    public class CheckoutPaymentRequest
    {
        public string Method { get; set; } = string.Empty;
        public decimal TenderedAmount { get; set; }
        public decimal AppliedAmount { get; set; }
        public decimal ChangeApplied { get; set; }
    }
}
