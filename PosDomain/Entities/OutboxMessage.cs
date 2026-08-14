using System;

namespace PosDomain.Entities
{
    public class OutboxMessage
    {
        public int Id { get; set; }
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string TenantId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string AggregateId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string SchemaVersion { get; set; } = "1.0";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int AttemptCount { get; set; } = 0;
        public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public string LastError { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Processing, Processed, Failed, DeadLetter
    }
}
