import re

with open('PosInfrastructure/Services/Local/OrderManagementService.cs', 'r') as f:
    content = f.read()

outbox_code = """
                // 8. Create Outbox Message
                var outboxMessage = new OutboxMessage
                {
                    EventId = Guid.NewGuid().ToString(),
                    TenantId = tenantId,
                    DeviceId = Environment.MachineName, // Simple fallback, or whatever the actual device ID should be
                    AggregateId = order.Id.ToString(),
                    EventType = "OrderCompleted",
                    Payload = System.Text.Json.JsonSerializer.Serialize(new { OrderId = order.Id, TotalAmount = order.TotalAmount }),
                    SchemaVersion = "1.0",
                    CreatedAt = DateTime.UtcNow,
                    AttemptCount = 0,
                    NextAttemptAt = DateTime.UtcNow,
                    Status = "Pending"
                };
                _dbContext.OutboxMessages.Add(outboxMessage);

                await _dbContext.SaveChangesAsync();
"""

content = content.replace("await _dbContext.SaveChangesAsync();\n                await transaction.CommitAsync();", outbox_code + "                await transaction.CommitAsync();")

with open('PosInfrastructure/Services/Local/OrderManagementService.cs', 'w') as f:
    f.write(content)
