namespace PosApplication.Interfaces.Server;

public interface ITenantContext
{
    void SetTenantId(string tenantId);
    string GetTenantId();
    void SetUsername(string username);
    string GetUsername();
    string GetUserId();
    void SetUserId(string userId);
}
