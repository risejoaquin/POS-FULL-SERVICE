with open('./PosCore/ViewModels/UsersViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
    "    public UsersViewModel(PosDbContext dbContext)\n    {\n        _dbContext = dbContext;\n        LoadUsers();\n    }",
    "    private readonly PosCore.Services.SessionManager _sessionManager;\n\n    public UsersViewModel(PosDbContext dbContext, PosCore.Services.SessionManager sessionManager)\n    {\n        _dbContext = dbContext;\n        _sessionManager = sessionManager;\n        LoadUsers();\n    }"
)

old_reset = """    private void ResetPin(User user)
    {
        if (user == null) return;

        var dialog = new PosCore.Views.ManagerOverrideWindow("Restablecer PIN de usuario", _dbContext);
        if (dialog.ShowDialog() == true && dialog.IsAuthorized)
        {"""

new_reset = """    private void ResetPin(User user)
    {
        if (user == null) return;

        bool authorized = false;
        if (_sessionManager.CurrentUserRole == "Admin")
        {
            authorized = true;
        }
        else
        {
            var dialog = new PosCore.Views.ManagerOverrideWindow("Restablecer PIN de usuario", _dbContext);
            authorized = dialog.ShowDialog() == true && dialog.IsAuthorized;
        }

        if (authorized)
        {"""

content = content.replace(old_reset, new_reset)

with open('./PosCore/ViewModels/UsersViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
