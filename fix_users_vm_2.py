import re

with open('./PosCore/ViewModels/UsersViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("public UsersViewModel(PosDbContext dbContext)\n    {\n        _dbContext = dbContext;", "private readonly PosCore.Services.SessionManager _sessionManager;\n\n    public UsersViewModel(PosDbContext dbContext, PosCore.Services.SessionManager sessionManager)\n    {\n        _dbContext = dbContext;\n        _sessionManager = sessionManager;")

content = re.sub(
    r'var dialog = new PosCore.Views.ManagerOverrideWindow[^;]+;\s*if \(dialog\.ShowDialog\(\) == true && dialog\.IsAuthorized\)',
    'bool authorized = _sessionManager.CurrentUserRole == "Admin";\n        if (!authorized) {\n            var dialog = new PosCore.Views.ManagerOverrideWindow("Restablecer Contraseña de usuario", _dbContext);\n            authorized = dialog.ShowDialog() == true && dialog.IsAuthorized;\n        }\n        if (authorized)',
    content
)

content = content.replace('Restablecer PIN', 'Restablecer Contraseña')
content = content.replace('El PIN de', 'La contraseña de')
content = content.replace('"PIN Restablecido"', '"Contraseña Restablecida"')

with open('./PosCore/ViewModels/UsersViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
