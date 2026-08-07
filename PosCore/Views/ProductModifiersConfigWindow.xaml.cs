using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PosCore.Data;
using PosCore.Models;

namespace PosCore.Views
{
    public partial class ProductModifiersConfigWindow : Window
    {
        private PosDbContext _dbContext;
        private Product _product;
        private List<ProductModifier> _allModifiers = new();
        private List<ProductModifier> _productModifiers = new();

        public ProductModifiersConfigWindow(Product product, PosDbContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _product = product;
            LoadData();
        }

        private void LoadData()
        {
            _allModifiers = _dbContext.ProductModifiers.ToList();
            
            var existingLinks = _dbContext.ProductModifierLinks
                .Include(l => l.ProductModifier)
                .Where(l => l.ProductId == _product.Id)
                .OrderBy(l => l.SortOrder)
                .ToList();
                
            _productModifiers = existingLinks.Select(l => l.ProductModifier).ToList();

            UpdateLists();
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

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Remove existing links
            var existingLinks = _dbContext.ProductModifierLinks.Where(l => l.ProductId == _product.Id).ToList();
            _dbContext.ProductModifierLinks.RemoveRange(existingLinks);
            
            // Add new links
            for (int i = 0; i < _productModifiers.Count; i++)
            {
                var mod = _productModifiers[i];
                var link = new ProductModifierLink
                {
                    ProductId = _product.Id,
                    ProductModifierId = mod.Id,
                    SortOrder = i
                };
                _dbContext.ProductModifierLinks.Add(link);
            }
            
            try
            {
                _dbContext.SaveChanges();
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
