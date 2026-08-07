using System;
using System.Windows;
using PosCore.Data;
using PosCore.Models;

namespace PosCore.Views
{
    public partial class SupplyEditorWindow : Window
    {
        private PosDbContext _dbContext;
        private Supply _supply;
        private bool _isNew;

        public SupplyEditorWindow(PosDbContext dbContext, Supply? supply = null)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _isNew = supply == null;
            _supply = supply ?? new Supply();

            if (!_isNew)
            {
                TitleText.Text = "Editar Insumo";
                TxtName.Text = _supply.Name;
                TxtUnit.Text = _supply.UnitOfMeasure;
                TxtCost.Text = _supply.Cost.ToString("F2");
                TxtStock.Text = _supply.Stock.ToString("F3");
                TxtMinStock.Text = _supply.MinStockThreshold.ToString("F3");
            }
            else
            {
                TxtUnit.Text = "kg";
                TxtCost.Text = "0.00";
                TxtStock.Text = "0.00";
                TxtMinStock.Text = "0.00";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("El nombre es requerido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtCost.Text, out decimal cost) || cost < 0)
            {
                MessageBox.Show("Costo inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtStock.Text, out decimal stock))
            {
                MessageBox.Show("Stock inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtMinStock.Text, out decimal minStock) || minStock < 0)
            {
                MessageBox.Show("Stock mínimo inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _supply.Name = TxtName.Text.Trim();
            _supply.UnitOfMeasure = TxtUnit.Text.Trim();
            _supply.Cost = cost;
            _supply.Stock = stock;
            _supply.MinStockThreshold = minStock;

            try
            {
                if (_isNew)
                {
                    _dbContext.Supplies.Add(_supply);
                }
                else
                {
                    _dbContext.Supplies.Update(_supply);
                }
                
                _dbContext.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
