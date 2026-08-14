using System.Threading.Tasks;

namespace PosApplication.Interfaces.Local
{
    public interface IAuditService
    {
        Task LogActionAsync(string action, string entityType, string entityId, string oldValues, string newValues, string tenantId, string userId = "", string correlationId = "");
    }
}
