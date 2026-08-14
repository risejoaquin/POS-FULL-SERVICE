using System;

namespace PosDomain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string CorrelationId { get; set; } = string.Empty;
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
    }
}
