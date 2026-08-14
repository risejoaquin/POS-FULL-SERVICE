namespace PosApplication.DTOs.Local
{
    public class CheckoutResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal ChangeDue { get; set; }
        public int? OrderId { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
        public string PaymentDetails { get; set; } = string.Empty;

        public static CheckoutResult Success(int orderId, decimal changeDue, string idempotencyKey, string paymentDetails)
        {
            return new CheckoutResult
            {
                IsSuccess = true,
                Message = "Venta completada exitosamente.",
                OrderId = orderId,
                ChangeDue = changeDue,
                IdempotencyKey = idempotencyKey,
                PaymentDetails = paymentDetails
            };
        }

        public static CheckoutResult Fail(string message, decimal changeDue = 0m, string idempotencyKey = "")
        {
            return new CheckoutResult
            {
                IsSuccess = false,
                Message = message,
                ChangeDue = changeDue,
                IdempotencyKey = idempotencyKey
            };
        }
    }
}
