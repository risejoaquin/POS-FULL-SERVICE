import re
with open('PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

props = """
    [ObservableProperty]
    private string _currentUserRole = string.Empty;
    
    [ObservableProperty]
    private string _currentUsername = string.Empty;
"""
content = content.replace("private readonly PosCore.Services.SessionManager _sessionManager;", props + "\n    private readonly PosCore.Services.SessionManager _sessionManager;")

init = """
        _sessionManager = sessionManager;
        IsAdmin = _sessionManager.Role != null && (_sessionManager.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || _sessionManager.Role.Equals("Administrador", StringComparison.OrdinalIgnoreCase));
        CurrentUserRole = _sessionManager.Role ?? "Administrador";
        CurrentUsername = _sessionManager.Username ?? "Usuario";
"""
content = re.sub(r'_sessionManager = sessionManager;\s*IsAdmin = _sessionManager\.Role != null && _sessionManager\.Role\.Equals\("Admin", StringComparison\.OrdinalIgnoreCase\);', init, content)

with open('PosCore/ViewModels/MainViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
print("MainViewModel.cs patched")
