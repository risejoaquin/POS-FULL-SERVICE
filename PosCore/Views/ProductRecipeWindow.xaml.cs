using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PosApplication.Interfaces.Local;
using PosDomain.Entities;
using System.Collections.ObjectModel;

namespace PosCore.Views
{
    public partial class ProductRecipeWindow : Window
    {
        private readonly IInventoryAppService _inventoryAppService;
        private readonly Product _product;
        private ObservableCollection<RecipeItemViewModel> _recipeItems = new();

        public ProductRecipeWindow(Product product, IInventoryAppService inventoryAppService)
        {
            InitializeComponent();
            _inventoryAppService = inventoryAppService;
            _product = product;
            
            TitleText.Text = $"Receta de: {_product.Name}";
            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var supplies = await _inventoryAppService.GetAllSuppliesAsync();
                CboSupplies.ItemsSource = supplies.ToList();

                var items = await _inventoryAppService.GetProductRecipeItemsAsync(_product.Id);
                _recipeItems = new ObservableCollection<RecipeItemViewModel>(items.Select(r => new RecipeItemViewModel(r)).ToList());
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

        private async void BtnAddSupply_Click(object sender, RoutedEventArgs e)
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

                try
                {
                    var newItem = await _inventoryAppService.AddRecipeItemAsync(_product.Id, supply.Id, quantity);
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

        private async void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is RecipeItemViewModel itemVm)
            {
                try
                {
                    await _inventoryAppService.DeleteRecipeItemAsync(itemVm.Model.Id);
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
