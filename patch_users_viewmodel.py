import re

with open('./PosCore/ViewModels/UsersViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Change the logic in ResetPin
old_reset = """    [RelayCommand]
    private async Task ResetPin(User user)
    {
        if (user == null) return;
        
        bool authorized = _sessionManager.Role == "Admin";
        if (!authorized)
        {"""

new_reset = """    [RelayCommand]
    private async Task ResetPin(User user)
    {
        if (user == null) return;
        
        bool authorized = _sessionManager.CurrentUser?.Role == "Admin" || _sessionManager.Role == "Admin" || _sessionManager.IsAdmin();
        if (!authorized)
        {"""

# Oh wait, let's see how SessionManager implements Role check.
