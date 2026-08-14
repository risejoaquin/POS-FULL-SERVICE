using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosApplication.DTOs.Local;
using PosApplication.Interfaces.Local;
using PosCore.Services;
using PosDomain.Entities;

namespace PosCore.ViewModels
{
    public partial class ReturnsViewModel : ObservableObject
    {
        private readonly IReturnsService _returnsService;
        private readonly IReceiptPrinter _ticketPrinterService;
        private readonly ILocalAuthService _localAuthService;

        [ObservableProperty]
        private ObservableCollection<Order> _orders = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        [ObservableProperty]
        private Order? _selectedOrder;

        [ObservableProperty]
        private string _notificationMessage = string.Empty;

        public ReturnsViewModel(IReturnsService returnsService, IReceiptPrinter ticketPrinterService, ILocalAuthService localAuthService)
        {
            _returnsService = returnsService;
            _ticketPrinterService = ticketPrinterService;
            _localAuthService = localAuthService;
            LoadOrdersCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadOrdersAsync()
        {
            try
            {
                var results = await _returnsService.SearchOrdersAsync(StartDate, EndDate, SearchQuery);
                Orders.Clear();
                foreach (var order in results)
                {
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar órdenes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void PrintCreditNote(Order order)
        {
            if (order == null || !order.IsReturned) return;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                try
                {
                    _ticketPrinterService.PrintCreditNote(order);
                    MessageBox.Show("Nota de crédito enviada a la impresora.", "Reimpresión", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al imprimir el ticket de devolución: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task PartialReturnOrderAsync(Order order)
        {
            if (order == null) return;
            if (order.IsReturned)
            {
                MessageBox.Show("Esta orden ya fue devuelta en su totalidad.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!await _returnsService.HasActiveShiftAsync())
            {
                MessageBox.Show("No hay un turno abierto. Por favor, abra un turno antes de realizar devoluciones.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var partialWindow = new PosCore.Views.PartialReturnWindow(order);
            if (partialWindow.ShowDialog() != true) return;

            var selectedItems = partialWindow.ReturnItems
                .Where(i => i.IsSelected)
                .Select(i => new ReturnItemRequest
                {
                    OrderItemId = i.OriginalItem.Id,
                    Quantity = i.ReturnQuantity
                })
                .ToList();

            var overrideWindow = new PosCore.Views.ManagerOverrideWindow("Devolución Parcial", _localAuthService);
            if (overrideWindow.ShowDialog() != true || !overrideWindow.IsAuthorized) return;

            var reasonWindow = new PosCore.Views.ReasonWindow();
            if (reasonWindow.ShowDialog() != true) return;

            try
            {
                await _returnsService.ProcessPartialReturnAsync(order.Id, selectedItems, reasonWindow.SelectedReason, overrideWindow.AuthorizedBy);
                var totalRefund = partialWindow.ReturnItems
                    .Where(i => i.IsSelected)
                    .Sum(i => i.UnitPrice * i.ReturnQuantity);

                MessageBox.Show($"Devolución parcial procesada. Reembolso: {totalRefund:C}", "Devolución Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ReturnOrderAsync(Order order)
        {
            if (order == null) return;
            if (order.IsReturned)
            {
                MessageBox.Show("Esta orden ya fue devuelta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!await _returnsService.HasActiveShiftAsync())
            {
                MessageBox.Show("No hay un turno abierto. Por favor, abra un turno antes de realizar devoluciones.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"¿Está seguro que desea devolver la orden {order.Id} por {order.TotalAmount:C}?\nEsto sumará los productos al inventario.", "Confirmar Devolución", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var overrideWindow = new PosCore.Views.ManagerOverrideWindow("Devolución de Orden", _localAuthService);
            if (overrideWindow.ShowDialog() != true || !overrideWindow.IsAuthorized)
            {
                return;
            }

            var reasonWindow = new PosCore.Views.ReasonWindow();
            if (reasonWindow.ShowDialog() != true)
            {
                return;
            }

            try
            {
                await _returnsService.ProcessFullReturnAsync(order.Id, reasonWindow.SelectedReason, overrideWindow.AuthorizedBy);
                MessageBox.Show("Orden devuelta y stock actualizado exitosamente.", "Devolución Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al devolver la orden: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
