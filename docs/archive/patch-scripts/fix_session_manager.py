with open("PosCore/Services/SessionManager.cs", "r") as f:
    text = f.read()

text = text.replace("public class SessionManager", "using PosDomain.Interfaces;\npublic class SessionManager : ITenantService")
text = text.replace("public string CurrentTenantId { get; set; } = string.Empty;", "public string CurrentTenantId { get; set; } = string.Empty;\n    public void SetTenantId(string tenantId) => CurrentTenantId = tenantId;\n    public string GetTenantId() => CurrentTenantId;")
text = text.replace("public string Username { get; set; } = string.Empty;", "public string Username { get; set; } = string.Empty;\n    public void SetUsername(string username) => Username = username;\n    public string GetUsername() => Username;")
text = text.replace("public string CurrentUserId { get; set; } = string.Empty;", "public string CurrentUserId { get; set; } = string.Empty;\n    public void SetUserId(string userId) => CurrentUserId = userId;\n    public string GetUserId() => CurrentUserId;")

with open("PosCore/Services/SessionManager.cs", "w") as f:
    f.write(text)
