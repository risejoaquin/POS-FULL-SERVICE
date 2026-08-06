using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PosCore.Data;
using PosCore.Models;

namespace PosCore.ViewModels;

public partial class UsersViewModel : ObservableObject
{
    private readonly PosDbContext _dbContext;

    [ObservableProperty]
    private ObservableCollection<User> _users = new();

    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newPin = string.Empty;

    [ObservableProperty]
    private string _newRole = "Cashier";

    public ObservableCollection<string> Roles { get; } = new ObservableCollection<string> { "Cashier", "Admin" };

    private readonly PosCore.Services.SessionManager _sessionManager;

    public UsersViewModel(PosDbContext dbContext, PosCore.Services.SessionManager sessionManager)
    {
        _dbContext = dbContext;
        _sessionManager = sessionManager;
        LoadUsers();
    }

    private void LoadUsers()
    {
        var localUsers = _dbContext.Users.ToList();
        Users.Clear();
        foreach (var u in localUsers) Users.Add(u);
    }

    [RelayCommand]
    private void AddUser()
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
            var existingUser = _dbContext.Users.FirstOrDefault(u => u.Username.ToLower() == NewUsername.ToLower() && u.Id != SelectedUser.Id);
            if (existingUser != null)
            {
                MessageBox.Show("Ya existe otro usuario con ese nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SelectedUser.Username = NewUsername;
            SelectedUser.Pin = NewPin;
            SelectedUser.Role = NewRole;
            _dbContext.Users.Update(SelectedUser);
            var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            _dbContext.OutboxMessages.Add(new OutboxMessage { EventType = "UserUpdated", Payload = System.Text.Json.JsonSerializer.Serialize(SelectedUser, jsonOptions), CreatedAt = DateTime.Now });
            SelectedUser = null;
        }
        else
        {
            var existingUser = _dbContext.Users.FirstOrDefault(u => u.Username.ToLower() == NewUsername.ToLower());
            if (existingUser != null)
            {
                MessageBox.Show("Ya existe un usuario con ese nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var user = new User
            {
                Username = NewUsername,
                Pin = NewPin,
                Role = NewRole
            };
            _dbContext.Users.Add(user);
            var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            _dbContext.OutboxMessages.Add(new OutboxMessage { EventType = "UserCreated", Payload = System.Text.Json.JsonSerializer.Serialize(user, jsonOptions), CreatedAt = DateTime.Now });
        }

        _dbContext.SaveChanges();

        LoadUsers();
        NewUsername = string.Empty;
        NewPin = string.Empty;
        NewRole = "Cashier";
    }


    [ObservableProperty]
    private User? _selectedUser;

    [RelayCommand]
    private void EditUser(User user)
    {
        if (user == null) return;
        SelectedUser = user;
        NewUsername = user.Username;
        NewPin = user.Pin;
        NewRole = user.Role;
    }

    [RelayCommand]
    private void ResetPin(User user)
    {
        if (user == null) return;
        bool authorized = _sessionManager.Role == "Admin";
        if (!authorized) {
            var dialog = new PosCore.Views.ManagerOverrideWindow("Restablecer Contraseña de usuario", _dbContext);
            authorized = dialog.ShowDialog() == true && dialog.IsAuthorized;
        }
        if (authorized)
        {
            user.Pin = "1234";
            _dbContext.Users.Update(user);
            var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            _dbContext.OutboxMessages.Add(new OutboxMessage { EventType = "UserUpdated", Payload = System.Text.Json.JsonSerializer.Serialize(user, jsonOptions), CreatedAt = DateTime.Now });
            _dbContext.SaveChanges();
            LoadUsers();
            MessageBox.Show($"La contraseña de {user.Username} ha sido restablecido a '1234'.", "Contraseña Restablecida", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void DeleteUser(User user)
    {
        if (user == null) return;
        
        var result = MessageBox.Show($"¿Eliminar al usuario {user.Username}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _dbContext.Users.Remove(user);
            var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            _dbContext.OutboxMessages.Add(new OutboxMessage { EventType = "UserDeleted", Payload = System.Text.Json.JsonSerializer.Serialize(user, jsonOptions), CreatedAt = DateTime.Now });
            _dbContext.SaveChanges();
            LoadUsers();
        }
    }
}
