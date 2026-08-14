using System.Threading.Tasks;
using PosApplication.DTOs.Local;

namespace PosApplication.Interfaces.Local
{
    public interface ISyncEngine
    {
        Task<SyncResult> TriggerSyncAsync();
        bool IsSyncing { get; }
        double SyncProgress { get; }
        string SyncStatusMessage { get; }
    }
}
