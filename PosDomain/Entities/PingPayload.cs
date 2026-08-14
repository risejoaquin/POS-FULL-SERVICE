using System;
namespace PosDomain.Entities
{
    public class PingPayload
    {
        public string? AppVersion { get; set; } = string.Empty;
        public double MemoryUsageMB { get; set; }
        public string? PrinterStatus { get; set; } = string.Empty;
        public string? LastSaleId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
