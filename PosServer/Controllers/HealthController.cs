using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosInfrastructure.Data.Server;
using PosDomain.Entities;
using System;
using System.Linq;

namespace PosServer.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly CentralDbContext _context;

        public HealthController(CentralDbContext context)
        {
            _context = context;
        }

        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                bool canConnect = await _context.Database.CanConnectAsync();
                if (canConnect)
                {
                    return Ok(new { status = "Healthy", database = "Connected", timestamp = DateTime.UtcNow });
                }
                
                return StatusCode(503, new { status = "Unhealthy", database = "Disconnected", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Database readiness check failed.");
                return StatusCode(503, new { status = "Unhealthy", database = "Error", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("metrics")]
        [HttpGet("/metrics")]
        public async Task<IActionResult> Metrics()
        {
            try
            {
                int syncPending = await _context.OutboxMessages.IgnoreQueryFilters().CountAsync(m => m.ProcessedAt == null);
                int syncFailed = await _context.OutboxMessages.IgnoreQueryFilters().CountAsync(m => m.Status == "Failed" || m.Status == "DeadLetter");
                
                var processedMessages = await _context.OutboxMessages.IgnoreQueryFilters()
                    .Where(m => m.ProcessedAt != null)
                    .Select(m => new { m.CreatedAt, m.ProcessedAt })
                    .Take(100)
                    .ToListAsync();
                
                double syncLatency = processedMessages.Any()
                    ? processedMessages.Average(m => (m.ProcessedAt!.Value - m.CreatedAt).TotalSeconds)
                    : 0.0;

                int ordersCreated = await _context.Orders.IgnoreQueryFilters().CountAsync();
                int ordersFailed = await _context.Orders.IgnoreQueryFilters().CountAsync(o => o.Status == OrderStatus.Cancelled);

                int inventoryConflicts = await _context.AuditLogs.IgnoreQueryFilters()
                    .CountAsync(l => l.Action.Contains("Conflict") || l.Action.Contains("conflict") || l.NewValues.Contains("Conflict") || l.NewValues.Contains("conflict"));

                int loginFailures = await _context.AuditLogs.IgnoreQueryFilters()
                    .CountAsync(l => l.Action == "LoginFailed" || l.Action == "LoginFailure" || l.Action.Contains("Login Failed"));

                var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);
                int activeTerminals = await _context.AuditLogs.IgnoreQueryFilters()
                    .Where(l => l.Timestamp >= fifteenMinutesAgo && !string.IsNullOrEmpty(l.DeviceId))
                    .Select(l => l.DeviceId)
                    .Distinct()
                    .CountAsync();

                if (activeTerminals == 0)
                {
                    activeTerminals = await _context.AuditLogs.IgnoreQueryFilters()
                        .Where(l => !string.IsNullOrEmpty(l.DeviceId))
                        .Select(l => l.DeviceId)
                        .Distinct()
                        .CountAsync();
                }

                return Ok(new
                {
                    sync_pending = syncPending,
                    sync_failed = syncFailed,
                    sync_latency = syncLatency,
                    orders_created = ordersCreated,
                    orders_failed = ordersFailed,
                    inventory_conflicts = inventoryConflicts,
                    login_failures = loginFailures,
                    active_terminals = activeTerminals,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to retrieve metrics.");
                return StatusCode(500, new { error = "Failed to retrieve metrics" });
            }
        }
    }
}
