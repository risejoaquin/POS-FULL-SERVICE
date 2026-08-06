import re

with open('./PosCore/ViewModels/UsersViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Change the logic in ResetPin
old_reset = """        bool authorized = _sessionManager.CurrentUser?.Role == "Admin" || _sessionManager.CurrentUser?.Role == "Administrador";"""

new_reset = """        bool authorized = _sessionManager.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || _sessionManager.Role.Equals("Administrador", StringComparison.OrdinalIgnoreCase);"""

content = content.replace(old_reset, new_reset)

with open('./PosCore/ViewModels/UsersViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
