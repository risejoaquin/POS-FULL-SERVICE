import re

with open('./PosCore/ViewModels/LoginViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
'''    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Ingrese usuario y contraseña";
            return;
        }''',
'''    [RelayCommand]
    private async Task LoginAsync()
    {
        Serilog.Log.Information("LoginAsync called. User: {User}, PasswordLength: {Len}", Username, Password?.Length ?? 0);
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Ingrese usuario y contraseña";
            Serilog.Log.Warning("Login failed locally: empty fields");
            return;
        }''')

with open('./PosCore/ViewModels/LoginViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
