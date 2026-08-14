using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PosApplication.Interfaces.Local;
using PosDomain.Entities;

namespace PosCore.Views
{
    public partial class ProductModifiersConfigWindow : Window
    {
        private readonly IInventoryAppService _inventoryAppService;
        private readonly Product _product;
        private List<ProductModifier> _allModifiers = new();
        private List<ProductModifier> _productModifiers = new();

        public ProductModifiersConfigWindow(Product product, IInventoryAppService inventoryAppService)
        {
            InitializeComponent();
            _inventoryAppService = inventoryAppService;
            _product = product;
            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _allModifiers = (await _inventoryAppService.GetAllProductModifiersAsync()).ToList();
                _productModifiers = (await _inventoryAppService.GetProductModifiersAsync(_product.Id)).ToList();
                UpdateLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar modificadores: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateLists()
        {
            var availableModifiers = _allModifiers.Where(m => !_productModifiers.Any(pm => pm.Id == m.Id)).ToList();
            
            AllModifiersList.ItemsSource = null;
            AllModifiersList.ItemsSource = availableModifiers;
            
            ProductModifiersList.ItemsSource = null;
            ProductModifiersList.ItemsSource = _productModifiers;
        }

        private void BtnAddModifier_Click(object sender, RoutedEventArgs e)
        {
            if (AllModifiersList.SelectedItem is ProductModifier mod)
            {
                _productModifiers.Add(mod);
                UpdateLists();
            }
        }

        private void BtnRemoveModifier_Click(object sender, RoutedEventArgs e)
        {
            if (ProductModifiersList.SelectedItem is ProductModifier mod)
            {
                _productModifiers.Remove(mod);
                UpdateLists();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _inventoryAppService.UpdateProductModifiersAsync(_product.Id, _productModifiers.Select(m => m.Id));
                MessageBox.Show("Modificadores guardados exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
