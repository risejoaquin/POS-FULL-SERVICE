using PosCore.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosApplication.DTOs.Local;
using PosApplication.Interfaces.Local;
using System;
using System.Threading.Tasks;

namespace PosCore.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ILocalAuthService _localAuthService;
    private readonly SessionManager _sessionManager;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public Action? RequestClose { get; set; }

    [ObservableProperty]
    private string _companyName = "POS Express";

    [ObservableProperty]
    private string _logoPath = "";

    public LoginViewModel(
        IApiService apiService,
        ILocalAuthService localAuthService,
        SessionManager sessionManager,
        Microsoft.Extensions.Options.IOptions<PosApplication.Models.AppSettings> appSettings)
    {
        _apiService = apiService;
        _localAuthService = localAuthService;
        _sessionManager = sessionManager;

        var settings = appSettings.Value;
        _companyName = settings.WhiteLabel.CompanyName;
        _logoPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, settings.WhiteLabel.LogoPath);
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        Serilog.Log.Information("LoginAsync called. User: {User}, PasswordLength: {Len}", Username, Password?.Length ?? 0);
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Ingrese usuario y contraseña";
            Serilog.Log.Warning("Login failed locally: empty fields");
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            if (await TryCloudLoginAsync())
            {
                return;
            }
        }
        catch (UnauthorizedAccessException uex)
        {
            ErrorMessage = uex.Message;
            IsLoading = false;
            return; // DO NOT fallback if cloud explicitly rejected
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Fallback to local error");
            // Fallback to local authentication when the cloud path is unavailable.
        }

        await TryLocalLoginAsync();
    }

    private async Task<bool> TryCloudLoginAsync()
    {
        var result = await _apiService.LoginAsync(Username, Password);
        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            return false;
        }

        var tenantId = string.IsNullOrEmpty(result.TenantId) ? "default" : result.TenantId;
        var role = string.IsNullOrEmpty(result.Role) ? "User" : result.Role;
        var localResult = await _localAuthService.CacheCloudLoginAsync(Username, Password, tenantId, role);

        _sessionManager.Token = result.Token;
        _sessionManager.CurrentTenantId = tenantId;
        _sessionManager.Username = Username;
        _sessionManager.Role = role;
        _sessionManager.CurrentUserId = localResult.CurrentUserId ?? localResult.User?.Id.ToString() ?? string.Empty;
        _sessionManager.SaveSession();

        CloseSuccessfulLogin();
        return true;
    }

    private async Task TryLocalLoginAsync()
    {
        var localResult = await _localAuthService.AuthenticateLocalUserAsync(Username, Password);
        if (localResult.IsSuccess)
        {
            ApplyLocalSession(localResult);
            CloseSuccessfulLogin();
            return;
        }

        ErrorMessage = "No se pudo iniciar sesión. Verifique sus credenciales o la conexión con el servidor.";
        IsLoading = false;
    }

    private void ApplyLocalSession(LoginResult localResult)
    {
        if (string.IsNullOrEmpty(_sessionManager.Token) || _sessionManager.Token.StartsWith("local-token"))
        {
            _sessionManager.Token = localResult.Token ?? $"local-token-{Guid.NewGuid()}";
        }

        _sessionManager.CurrentTenantId = localResult.TenantId ?? localResult.User?.TenantId ?? "default";
        _sessionManager.Username = localResult.Username ?? localResult.User?.Username ?? Username;
        _sessionManager.Role = localResult.Role ?? localResult.User?.Role ?? "User";
        _sessionManager.CurrentUserId = localResult.CurrentUserId ?? localResult.User?.Id.ToString() ?? string.Empty;
        _sessionManager.SaveSession();
    }

    private void CloseSuccessfulLogin()
    {
        RequestClose?.Invoke();
        IsLoading = false;
    }
}
