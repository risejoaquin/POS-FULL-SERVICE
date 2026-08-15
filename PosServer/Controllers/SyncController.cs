using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosDomain.Entities;
using PosApplication.Interfaces.Server;
using PosInfrastructure.Services.Server;
using PosDomain.Interfaces;
using System;
using System.Threading.Tasks;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;
        private readonly ITenantContext _tenantContext;

        public SyncController(ISyncService syncService, ITenantContext tenantContext)
        {
            _syncService = syncService;
            _tenantContext = tenantContext;
        }

        [HttpGet("changes")]
        public async Task<IActionResult> GetChanges([FromQuery] string? since)
        {
            var tenantId = _tenantContext.GetTenantId();
            DateTime sinceDateTime = DateTime.MinValue;
            if (!string.IsNullOrEmpty(since) && DateTime.TryParse(since, out var parsed))
            {
                sinceDateTime = parsed.ToUniversalTime();
            }

            var result = await _syncService.GetChangesAsync(tenantId, sinceDateTime);
            return Ok(result);
        }
        
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyChanges([FromBody] SyncPayload payload)
        {
            var tenantId = _tenantContext.GetTenantId();
            var success = await _syncService.ApplyChangesAsync(tenantId, payload);
            
            if (success)
                return Ok(new { Message = "Sincronización completada exitosamente." });
            else
                return BadRequest("Failed to apply changes");
        }

        [HttpPost("ping")]
        public IActionResult Ping([FromBody] PingPayload payload)
        {
            var tenantId = _tenantContext.GetTenantId();
            Serilog.Log.Information(
                "Heartbeat received for tenant {TenantId}. AppVersion={AppVersion}, MemoryUsageMB={MemoryUsageMB}, PrinterStatus={PrinterStatus}",
                tenantId,
                payload.AppVersion,
                payload.MemoryUsageMB,
                payload.PrinterStatus);
            return Ok();
        }
    }
}
