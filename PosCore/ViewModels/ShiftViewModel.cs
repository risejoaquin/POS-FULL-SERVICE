using System;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosCore.Data;
using PosCore.Models;
using PosCore.Services;

namespace PosCore.ViewModels;

public partial class ShiftViewModel : ObservableObject
{
    private readonly PosDbContext _dbContext;
    private readonly SessionManager _sessionManager;

    [ObservableProperty]
    private CashRegisterShift? _currentShift;

    [ObservableProperty]
    private decimal _startingCash = 0;

    [ObservableProperty]
    private decimal _actualEndingCash = 0;

    [ObservableProperty]
    private decimal _expectedEndingCash = 0;

    [ObservableProperty]
    private decimal _difference = 0;

    [ObservableProperty]
    private bool _hasActiveShift = false;

    private readonly TicketPrinterService _ticketPrinterService;
    public ShiftViewModel(PosDbContext dbContext, SessionManager sessionManager, TicketPrinterService ticketPrinterService)
    {
        _dbContext = dbContext;
        _sessionManager = sessionManager;
        _ticketPrinterService = ticketPrinterService;
        LoadCurrentShift();
    }

    private void LoadCurrentShift()
    {
        CurrentShift = _dbContext.CashRegisterShifts.FirstOrDefault(s => !s.IsClosed);
        HasActiveShift = CurrentShift != null;

        if (HasActiveShift && CurrentShift != null)
        {
            // Calculate Expected Ending Cash
            // Start with starting cash
            decimal cashSales = 0;
            var cashOrders = _dbContext.Orders
                .Where(o => o.OrderDate >= CurrentShift.OpenedAt && !o.IsReturned && o.PaymentDetails.Contains("Efectivo"))
                .AsEnumerable();
            
            foreach (var o in cashOrders)
            {
                var payments = o.PaymentDetails.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in payments)
                {
                    if (p.StartsWith("Efectivo: "))
                    {
                        var amountStr = p.Substring("Efectivo: ".Length);
                        if (decimal.TryParse(amountStr, System.Globalization.NumberStyles.Currency, null, out decimal amount))
                        {
                            cashSales += amount;
                        }
                    }
                }
            }
            
            // Add cash movements (in/out)
            decimal movements = _dbContext.CashMovements
                .Where(c => c.ShiftId == CurrentShift.Id)
                .AsEnumerable()
                .Sum(c => c.Amount);

            ExpectedEndingCash = CurrentShift.StartingCash + cashSales + movements;
            ActualEndingCash = ExpectedEndingCash; // Default to expected for easy closing
            CalculateDifference();
        }
    }

    partial void OnActualEndingCashChanged(decimal value)
    {
        CalculateDifference();
    }

    private void CalculateDifference()
    {
        Difference = ActualEndingCash - ExpectedEndingCash;
    }

    [RelayCommand]
    private void OpenShift(Window window)
    {
        if (HasActiveShift)
        {
            MessageBox.Show("Ya hay un turno abierto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var shift = new CashRegisterShift
        {
            OpenedAt = DateTime.Now,
            OpenedBy = string.IsNullOrEmpty(_sessionManager.Username) ? "Admin" : _sessionManager.Username,
            StartingCash = StartingCash,
            IsClosed = false
        };

        _dbContext.CashRegisterShifts.Add(shift);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
        _dbContext.OutboxMessages.Add(new OutboxMessage { EventType = "ShiftOpened", Payload = System.Text.Json.JsonSerializer.Serialize(shift, jsonOptions), CreatedAt = DateTime.Now });
        _dbContext.SaveChanges();

        MessageBox.Show($"Turno abierto con un fondo de {StartingCash:C}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        window.Close();
    }


    [RelayCommand]
    private void RegisterWithdrawal()
    {
        if (!HasActiveShift || CurrentShift == null) return;
        
        var dialog = new PosCore.Views.WithdrawalWindow();
        if (dialog.ShowDialog() == true && dialog.Amount > 0)
        {
            var movement = new CashMovement
            {
                ShiftId = CurrentShift.Id,
                Amount = -dialog.Amount,
                Type = "Salida",
                Reason = string.IsNullOrWhiteSpace(dialog.Reason) ? "Retiro Parcial" : dialog.Reason,
                CreatedAt = DateTime.Now
            };
            _dbContext.CashMovements.Add(movement);
            var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            
            // To sync movement we resync the shift with the new movement
            CurrentShift.Movements ??= new System.Collections.Generic.List<CashMovement>();
            CurrentShift.Movements.Add(movement);
            _dbContext.OutboxMessages.Add(new OutboxMessage { EventType = "CashMovementCreated", Payload = System.Text.Json.JsonSerializer.Serialize(CurrentShift, jsonOptions), CreatedAt = DateTime.Now });
            _dbContext.SaveChanges();
            
            MessageBox.Show($"Retiro de {dialog.Amount:C} registrado correctamente.", "Retiro de Efectivo", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadCurrentShift(); // Recalculate
        }
    }

    [RelayCommand]
    private void CloseShift(Window window)
    {
        if (!HasActiveShift || CurrentShift == null)
        {
            MessageBox.Show("No hay turno activo para cerrar.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        CurrentShift.ClosedAt = DateTime.Now;
        CurrentShift.ClosedBy = string.IsNullOrEmpty(_sessionManager.Username) ? "Admin" : _sessionManager.Username;
        CurrentShift.ExpectedEndingCash = ExpectedEndingCash;
        CurrentShift.ActualEndingCash = ActualEndingCash;
        CurrentShift.Difference = Difference;
        CurrentShift.IsClosed = true;

        var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
        _dbContext.OutboxMessages.Add(new OutboxMessage { EventType = "ShiftClosed", Payload = System.Text.Json.JsonSerializer.Serialize(CurrentShift, jsonOptions), CreatedAt = DateTime.Now });

        _dbContext.SaveChanges();

        MessageBox.Show($"Turno cerrado.\nEsperado: {ExpectedEndingCash:C}\nContado: {ActualEndingCash:C}\nDiferencia: {Difference:C}", "Arqueo de Caja", MessageBoxButton.OK, MessageBoxImage.Information);
        window.Close();
    }
}
