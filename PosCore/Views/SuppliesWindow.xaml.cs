using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PosCore.Data;
using PosCore.Models;

namespace PosCore.Views
{
    public partial class SuppliesWindow : Window
    {
        private PosDbContext _dbContext;

        public SuppliesWindow(PosDbContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var supplies = _dbContext.Supplies.OrderBy(s => s.Name).ToList();
                SuppliesDataGrid.ItemsSource = supplies;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar insumos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNewSupply_Click(object sender, RoutedEventArgs e)
        {
            var window = new SupplyEditorWindow(_dbContext);
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void BtnEditSupply_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var supply = button?.DataContext as Supply;
            if (supply != null)
            {
                var window = new SupplyEditorWindow(_dbContext, supply);
                window.Owner = this;
                if (window.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
