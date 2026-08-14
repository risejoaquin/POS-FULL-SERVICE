using PosDomain.Interfaces;
using System.Threading;

using PosApplication.Interfaces.Server;

namespace PosApplication.Interfaces.Server;

public class TenantContext : ITenantContext
{
    private static readonly AsyncLocal<string> _tenantId = new AsyncLocal<string>();
    private static readonly AsyncLocal<string> _username = new AsyncLocal<string>();
    private static readonly AsyncLocal<string> _userId = new AsyncLocal<string>();

    public void SetTenantId(string tenantId) => _tenantId.Value = tenantId;
    public string GetTenantId() => _tenantId.Value ?? string.Empty;

    public void SetUsername(string username) => _username.Value = username;
    public string GetUsername() => _username.Value ?? string.Empty;

    public void SetUserId(string userId) => _userId.Value = userId;
    public string GetUserId() => _userId.Value ?? string.Empty;
}
