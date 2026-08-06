import re

with open('./PosCore/ViewModels/LoginViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

if "public string CompanyName" not in content:
    content = content.replace(
        "private readonly PosCore.Data.PosDbContext _dbContext;",
        "private readonly PosCore.Data.PosDbContext _dbContext;\n\n    [ObservableProperty]\n    private string _companyName = \"POS Express\";\n\n    [ObservableProperty]\n    private string _logoPath = \"\";"
    )
    content = content.replace(
        "public LoginViewModel(IApiService apiService, SessionManager sessionManager, PosCore.Data.PosDbContext dbContext)",
        "public LoginViewModel(IApiService apiService, SessionManager sessionManager, PosCore.Data.PosDbContext dbContext, Microsoft.Extensions.Options.IOptions<PosCore.Models.AppSettings> appSettings)"
    )
    content = content.replace(
        "_dbContext = dbContext;",
        "_dbContext = dbContext;\n        var settings = appSettings.Value;\n        _companyName = settings.WhiteLabel.CompanyName;\n        _logoPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, settings.WhiteLabel.LogoPath);"
    )
    with open('./PosCore/ViewModels/LoginViewModel.cs', 'w', encoding='utf-8') as f:
        f.write(content)

with open('./PosCore/Views/LoginWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('Title="Iniciar Sesión - POS Express"', 'Title="{Binding CompanyName, StringFormat=\'Iniciar Sesión - {0}\'}"')

content = content.replace(
    '<TextBlock Text="Bienvenido a POS" FontSize="28" FontWeight="Bold" HorizontalAlignment="Center" Margin="0,0,0,5" Foreground="{StaticResource TextPrimaryBrush}"/>',
    '<Image Source="{Binding LogoPath}" MaxHeight="60" Margin="0,0,0,10" HorizontalAlignment="Center" />\n                <TextBlock Text="{Binding CompanyName, StringFormat=\'Bienvenido a {0}\'}" FontSize="28" FontWeight="Bold" HorizontalAlignment="Center" Margin="0,0,0,5" Foreground="{StaticResource TextPrimaryBrush}"/>'
)

# And PasswordBox Helper! But LoginWindow uses TextBox?
content = content.replace(
    '<TextBox Text="{Binding Password',
    '<PasswordBox local:PasswordBoxHelper.BindPassword="True" local:PasswordBoxHelper.BoundPassword="{Binding Password'
)
content = content.replace(
    '<TextBlock Text="Contraseña / PIN"',
    '<TextBlock Text="Contraseña"'
)
content = content.replace('</TextBox>', '</PasswordBox>')
content = content.replace('<TextBox.Resources>', '<PasswordBox.Resources>')
content = content.replace('</TextBox.Resources>', '</PasswordBox.Resources>')

with open('./PosCore/Views/LoginWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
