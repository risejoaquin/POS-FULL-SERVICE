using System;
using System.ComponentModel.DataAnnotations;

namespace PosDomain.Entities
{
    public class InboxMessage
    {
        [Key]
        public string EventId { get; set; } = string.Empty;
        
        public string? TenantId { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
