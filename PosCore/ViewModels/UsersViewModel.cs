using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosApplication.Interfaces.Local;
using PosCore.Services;
using PosDomain.Entities;
using System;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;

namespace PosCore.ViewModels;

public partial class UsersViewModel : ObservableObject
{
    private readonly IUsersService _usersService;
    private readonly ILocalAuthService _localAuthService;
    private readonly SessionManager _sessionManager;

    [ObservableProperty]
    private ObservableCollection<User> _users = new();

    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newPin = string.Empty;

    [ObservableProperty]
    private string _newRole = "Cashier";

    [ObservableProperty]
    private User? _selectedUser;

    public ObservableCollection<string> Roles { get; } = new() { "Cashier", "Admin" };

    public UsersViewModel(IUsersService usersService, SessionManager sessionManager, ILocalAuthService localAuthService)
    {
        _usersService = usersService;
        _sessionManager = sessionManager;
        _localAuthService = localAuthService;
        _ = LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        var localUsers = await _usersService.GetAllUsersAsync();
        Users.Clear();
        foreach (var user in localUsers)
        {
            Users.Add(user);
        }
    }

    [RelayCommand]
    private async Task AddUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPin) || string.IsNullOrWhiteSpace(NewRole))
        {
            MessageBox.Show("Por favor complete todos los campos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (NewPin.Length < 4)
        {
            MessageBox.Show("La contraseña debe tener al menos 4 dígitos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedUser != null)
        {
            var existingUser = await _usersService.UsernameExistsAsync(NewUsername, SelectedUser.Id);
            if (existingUser)
            {
                MessageBox.Show("Ya existe otro usuario con ese nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _usersService.UpdateUserAsync(SelectedUser, NewUsername, NewPin, NewRole);
            SelectedUser = null;
        }
        else
        {
            var existingUser = await _usersService.UsernameExistsAsync(NewUsername);
            if (existingUser)
            {
                MessageBox.Show("Ya existe un usuario con ese nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _usersService.CreateUserAsync(NewUsername, NewPin, NewRole);
        }

        await LoadUsersAsync();
        NewUsername = string.Empty;
        NewPin = string.Empty;
        NewRole = "Cashier";
    }

    [RelayCommand]
    private void EditUser(User? user)
    {
        if (user == null)
        {
            return;
        }

        SelectedUser = user;
        NewUsername = user.Username ?? string.Empty;
        NewPin = user.Pin ?? string.Empty;
        NewRole = string.IsNullOrWhiteSpace(user.Role) ? "Cashier" : user.Role;
    }

    [RelayCommand]
    private async Task ResetPinAsync(User? user)
    {
        if (user == null)
        {
            return;
        }

        var authorized = IsCurrentUserAdmin();
        if (!authorized)
        {
            var dialog = new PosCore.Views.ManagerOverrideWindow("Restablecer Contraseña de usuario", _localAuthService);
            authorized = dialog.ShowDialog() == true && dialog.IsAuthorized;
        }

        if (!authorized)
        {
            return;
        }

        var temporaryPin = GenerateTemporaryPin();
        await _usersService.ResetPinAsync(user.Id, temporaryPin);
        await LoadUsersAsync();
        MessageBox.Show($"La contraseña de {user.Username} ha sido restablecida. PIN temporal: {temporaryPin}", "Contraseña Restablecida", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task DeleteUserAsync(User? user)
    {
        if (user == null)
        {
            return;
        }

        var result = MessageBox.Show($"¿Eliminar al usuario {user.Username}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        await _usersService.DeleteUserAsync(user.Id);
        await LoadUsersAsync();
    }

    private bool IsCurrentUserAdmin()
    {
        return string.Equals(_sessionManager.Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(_sessionManager.Role, "Administrador", StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateTemporaryPin()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString(CultureInfo.InvariantCulture);
    }
}
