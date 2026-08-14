with open("PosInfrastructure/Data/Local/PosDbContextFactory.cs", "r") as f:
    text = f.read()

text = text.replace("using PosCore.Services;", "using PosDomain.Interfaces;")
text = text.replace("var sessionManager = new SessionManager();", "var sessionManager = new DesignTimeTenantService();")

text += """
public class DesignTimeTenantService : ITenantService
{
    private string _tenantId = string.Empty;
    public void SetTenantId(string tenantId) => _tenantId = tenantId;
    public string GetTenantId() => _tenantId;
    public void SetUsername(string username) { }
    public string GetUsername() => string.Empty;
    public void SetUserId(string userId) { }
    public string GetUserId() => string.Empty;
}
"""

with open("PosInfrastructure/Data/Local/PosDbContextFactory.cs", "w") as f:
    f.write(text)
