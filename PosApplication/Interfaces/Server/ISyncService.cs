using PosDomain.Interfaces;
using System.Threading.Tasks;
using PosDomain.Entities;
using System;

namespace PosApplication.Interfaces.Server
{
    public interface ISyncService
    {
        Task<object> GetChangesAsync(string tenantId, DateTime sinceDateTime);
        Task<bool> ApplyChangesAsync(string tenantId, SyncPayload payload);
    }
}
