using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PosApplication.Interfaces.Local;
using PosDomain.Entities;

namespace PosCore.Views
{
    public partial class SuppliesWindow : Window
    {
        private readonly IInventoryAppService _inventoryAppService;

        public SuppliesWindow(IInventoryAppService inventoryAppService)
        {
            InitializeComponent();
            _inventoryAppService = inventoryAppService;
            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var supplies = await _inventoryAppService.GetAllSuppliesAsync();
                SuppliesDataGrid.ItemsSource = supplies.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar insumos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnNewSupply_Click(object sender, RoutedEventArgs e)
        {
            var window = new SupplyEditorWindow(_inventoryAppService);
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnEditSupply_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var supply = button?.DataContext as Supply;
            if (supply != null)
            {
                var window = new SupplyEditorWindow(_inventoryAppService, supply);
                window.Owner = this;
                if (window.ShowDialog() == true)
                {
                    await LoadDataAsync();
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
