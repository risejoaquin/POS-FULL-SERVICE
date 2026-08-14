using PosCore.Services;
using System;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosDomain.Entities;
using PosApplication.Interfaces.Local;

namespace PosCore.ViewModels;

public partial class ShiftViewModel : ObservableObject
{
    private readonly IShiftService _shiftService;
    private readonly SessionManager _sessionManager;
    private readonly IReceiptPrinter _ticketPrinterService;
    private readonly ILocalAuthService _localAuthService;

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

    public ShiftViewModel(IShiftService shiftService, ILocalAuthService localAuthService, SessionManager sessionManager, IReceiptPrinter ticketPrinterService)
    {
        _shiftService = shiftService;
        _localAuthService = localAuthService;
        _sessionManager = sessionManager;
        _ticketPrinterService = ticketPrinterService;
        LoadCurrentShift();
    }

    private void LoadCurrentShift()
    {
        CurrentShift = _shiftService.GetActiveShiftAsync().GetAwaiter().GetResult();
        HasActiveShift = CurrentShift != null;

        if (HasActiveShift && CurrentShift != null)
        {
            var summary = _shiftService.GetShiftSummaryAsync(CurrentShift.Id).GetAwaiter().GetResult();
            ExpectedEndingCash = summary.ExpectedEndingCash;
            ActualEndingCash = 0; // Blind close: must count manually
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

        var openedBy = string.IsNullOrEmpty(_sessionManager.Username) ? "Admin" : _sessionManager.Username;

        try
        {
            CurrentShift = _shiftService.OpenShiftAsync(openedBy, StartingCash).GetAwaiter().GetResult();
            HasActiveShift = CurrentShift != null;
            MessageBox.Show($"Turno abierto con un fondo de {StartingCash:C}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            window.Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error al abrir el turno: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RegisterWithdrawal()
    {
        if (!HasActiveShift || CurrentShift == null) return;
        
        var dialog = new PosCore.Views.WithdrawalWindow();

        if (dialog.ShowDialog() == true && dialog.Amount > 0)
        {
            if (dialog.Amount > ExpectedEndingCash)
            {
                MessageBox.Show($"Fondos insuficientes en caja. Esperado actual: {ExpectedEndingCash:C}", "Retiro Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var tenantId = _sessionManager.CurrentTenantId;
                var createdBy = _sessionManager.CurrentUserId;
                
                _shiftService.RegisterCashMovementAsync(
                    CurrentShift.Id, 
                    -dialog.Amount, 
                    "Salida", 
                    string.IsNullOrWhiteSpace(dialog.Reason) ? "Retiro Parcial" : dialog.Reason, 
                    createdBy, 
                    tenantId).GetAwaiter().GetResult();
                
                MessageBox.Show($"Retiro de {dialog.Amount:C} registrado correctamente.", "Retiro de Efectivo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCurrentShift(); // Recalculate
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al registrar el retiro: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void PrintXReport()
    {
        if (!HasActiveShift || CurrentShift == null) return;
        GenerateAndPrintReport("CORTE X (PARCIAL)");
    }

    [RelayCommand]
    private void CloseShift(Window window)
    {
        if (!HasActiveShift || CurrentShift == null)
        {
            MessageBox.Show("No hay turno activo para cerrar.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        CalculateDifference(); // Ensure diff is calculated based on current input

        if (Math.Abs(Difference) > 0)
        {
            var msg = $"Diferencia de caja detectada: {Difference:C}\n¿Desea solicitar autorización para cerrar con descuadre?";
            if (MessageBox.Show(msg, "Arqueo Descuadrado", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var overrideWindow = new PosCore.Views.ManagerOverrideWindow("Cierre de Caja Descuadrada", _localAuthService);
                if (overrideWindow.ShowDialog() != true || !overrideWindow.IsAuthorized)
                {
                    return; // Cancel close
                }
                
                var tenantId = _sessionManager.CurrentTenantId;
                var createdBy = _sessionManager.CurrentUserId;
                
                _shiftService.RegisterCashMovementAsync(
                    CurrentShift.Id, 
                    Difference, 
                    Difference > 0 ? "Entrada" : "Salida", 
                    "Ajuste por descuadre en cierre de caja", 
                    createdBy, 
                    tenantId).GetAwaiter().GetResult();
            }
            else
            {
                return; // Cancel close
            }
        }

        var closedBy = _sessionManager.CurrentUserId;

        try
        {
            _shiftService.CloseShiftAsync(CurrentShift.Id, closedBy, ActualEndingCash, ExpectedEndingCash, Difference).GetAwaiter().GetResult();
            
            try
            {
                GenerateAndPrintReport("CORTE Z (FINAL)");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Turno cerrado, pero falló la impresión: {ex.Message}", "Error de Impresora", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            MessageBox.Show($"Turno cerrado.\nEsperado: {ExpectedEndingCash:C}\nContado: {ActualEndingCash:C}\nDiferencia: {Difference:C}", "Arqueo de Caja", MessageBoxButton.OK, MessageBoxImage.Information);
            window.Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error al cerrar el turno: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GenerateAndPrintReport(string reportType)
    {
        if (CurrentShift == null) return;
        
        try
        {
            var summary = _shiftService.GetShiftSummaryAsync(CurrentShift.Id).GetAwaiter().GetResult();

            _ticketPrinterService.PrintAccountingReport(
                reportType, 
                CurrentShift, 
                summary.CashSales, 
                summary.CardSales, 
                summary.TotalTaxes, 
                summary.CashIn, 
                summary.CashOut, 
                ExpectedEndingCash, 
                ActualEndingCash, 
                Difference, 
                summary.AnnulledCount, 
                summary.AnnulledTotal);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error al generar el reporte de impresión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
