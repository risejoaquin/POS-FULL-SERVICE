using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PosCore.Data;
using PosCore.Models;
using System.Collections.ObjectModel;

namespace PosCore.Views
{
    public partial class ProductRecipeWindow : Window
    {
        private PosDbContext _dbContext;
        private Product _product;
        private ObservableCollection<RecipeItemViewModel> _recipeItems = new();

        public ProductRecipeWindow(Product product, PosDbContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _product = product;
            
            TitleText.Text = $"Receta de: {_product.Name}";
            
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var supplies = _dbContext.Supplies.OrderBy(s => s.Name).ToList();
                CboSupplies.ItemsSource = supplies;

                var items = _dbContext.RecipeItems
                    .Include(r => r.Supply)
                    .Where(r => r.ProductId == _product.Id)
                    .ToList()
                    .Select(r => new RecipeItemViewModel(r))
                    .ToList();
                    
                _recipeItems = new ObservableCollection<RecipeItemViewModel>(items);
                RecipeDataGrid.ItemsSource = _recipeItems;
                
                CalculateTotalCost();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar receta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CalculateTotalCost()
        {
            decimal total = _recipeItems.Sum(r => r.CostoEstimado);
            TotalCostText.Text = total.ToString("C2");
        }

        private void BtnAddSupply_Click(object sender, RoutedEventArgs e)
        {
            if (CboSupplies.SelectedItem is Supply supply)
            {
                if (!decimal.TryParse(TxtQuantity.Text, out decimal quantity) || quantity <= 0)
                {
                    MessageBox.Show("Por favor, ingrese una cantidad válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (_recipeItems.Any(r => r.Supply.Id == supply.Id))
                {
                    MessageBox.Show("Este insumo ya está en la receta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var newItem = new RecipeItem
                {
                    ProductId = _product.Id,
                    SupplyId = supply.Id,
                    Quantity = quantity
                };

                try
                {
                    _dbContext.RecipeItems.Add(newItem);
                    _dbContext.SaveChanges();
                    
                    // Reload to get Include data
                    newItem.Supply = supply;
                    _recipeItems.Add(new RecipeItemViewModel(newItem));
                    
                    TxtQuantity.Text = "";
                    CalculateTotalCost();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al agregar insumo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un insumo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is RecipeItemViewModel itemVm)
            {
                try
                {
                    _dbContext.RecipeItems.Remove(itemVm.Model);
                    _dbContext.SaveChanges();
                    
                    _recipeItems.Remove(itemVm);
                    CalculateTotalCost();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al quitar insumo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class RecipeItemViewModel
    {
        public RecipeItem Model { get; }
        public Supply Supply => Model.Supply;
        public decimal Quantity => Model.Quantity;
        public decimal CostoEstimado => Model.Quantity * (Model.Supply?.Cost ?? 0);

        public RecipeItemViewModel(RecipeItem model)
        {
            Model = model;
        }
    }
}
