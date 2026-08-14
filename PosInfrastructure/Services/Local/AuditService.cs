using System;
using System.Threading.Tasks;
using PosDomain.Entities;
using PosInfrastructure.Data.Local;
using PosApplication.Interfaces.Local;

namespace PosInfrastructure.Services.Local
{
    public class AuditService : IAuditService
    {
        private readonly PosDbContext _dbContext;

        public AuditService(PosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogActionAsync(string action, string entityType, string entityId, string oldValues, string newValues, string tenantId, string userId = "", string correlationId = "")
        {
            var log = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues ?? string.Empty,
                NewValues = newValues ?? string.Empty,
                TenantId = tenantId,
                UserId = userId,
                DeviceId = Environment.MachineName,
                CorrelationId = string.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId,
                Timestamp = DateTime.UtcNow
            };

            _dbContext.AuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }
    }
}
