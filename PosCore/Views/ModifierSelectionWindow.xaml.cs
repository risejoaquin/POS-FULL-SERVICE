using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PosDomain.Entities;

namespace PosCore.Views
{
    public partial class ModifierSelectionWindow : Window
    {
        private decimal _basePrice;
        public decimal FinalPrice { get; private set; }
        public List<object> SelectedModifiers { get; private set; } = new();

        private List<ProductModifier> _modifiers;
        private List<CheckBox> _allCheckboxes = new();
        private List<RadioButton> _allRadioButtons = new();

        public ModifierSelectionWindow(Product product, List<ProductModifier> modifiers)
        {
            InitializeComponent();
            _basePrice = product.Price;
            _modifiers = modifiers;

            ProductNameText.Text = product.Name;
            ProductPriceText.Text = $"${_basePrice:F2}";

            RenderModifiers();
            UpdateTotal();
        }

        private void RenderModifiers()
        {
            foreach (var modGroup in _modifiers)
            {
                var groupBorder = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(10, 5, 10, 10),
                    Padding = new Thickness(15)
                };

                var groupStack = new StackPanel();

                var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
                var titleText = new TextBlock
                {
                    Text = modGroup.Name,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111827"))
                };
                
                var requirementText = new TextBlock
                {
                    Text = modGroup.IsRequired ? "Obligatorio" : "Opcional",
                    FontSize = 12,
                    Foreground = modGroup.IsRequired ? Brushes.Red : Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };

                headerGrid.Children.Add(titleText);
                headerGrid.Children.Add(requirementText);
                groupStack.Children.Add(headerGrid);

                bool isSingleSelection = modGroup.MaxSelections == 1;

                foreach (var option in modGroup.Options.OrderBy(o => o.SortOrder))
                {
                    var optionGrid = new Grid { Margin = new Thickness(0, 5, 0, 5) };
                    optionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    optionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var priceText = new TextBlock
                    {
                        Text = option.PriceAdjustment > 0 ? $"+${option.PriceAdjustment:F2}" : "",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(priceText, 1);

                    if (isSingleSelection)
                    {
                        var rb = new RadioButton
                        {
                            Content = option.Name,
                            GroupName = modGroup.Name,
                            IsChecked = option.IsDefault,
                            VerticalAlignment = VerticalAlignment.Center,
                            Tag = option
                        };
                        rb.Checked += (s, e) => UpdateTotal();
                        _allRadioButtons.Add(rb);
                        optionGrid.Children.Add(rb);
                    }
                    else
                    {
                        var cb = new CheckBox
                        {
                            Content = option.Name,
                            IsChecked = option.IsDefault,
                            VerticalAlignment = VerticalAlignment.Center,
                            Tag = option
                        };
                        cb.Checked += (s, e) => UpdateTotal();
                        cb.Unchecked += (s, e) => UpdateTotal();
                        _allCheckboxes.Add(cb);
                        optionGrid.Children.Add(cb);
                    }
                    
                    optionGrid.Children.Add(priceText);
                    groupStack.Children.Add(optionGrid);
                }

                groupBorder.Child = groupStack;
                ModifiersPanel.Children.Add(groupBorder);
            }
        }

        private void UpdateTotal()
        {
            decimal total = _basePrice;

            foreach (var rb in _allRadioButtons.Where(r => r.IsChecked == true))
            {
                var opt = (ModifierOption)rb.Tag;
                total += opt.PriceAdjustment;
            }

            foreach (var cb in _allCheckboxes.Where(c => c.IsChecked == true))
            {
                var opt = (ModifierOption)cb.Tag;
                total += opt.PriceAdjustment;
            }

            FinalPrice = total;

            if (TotalPriceText != null)
                TotalPriceText.Text = $"${total:F2}";
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Validate required groups
            foreach (var modGroup in _modifiers.Where(m => m.IsRequired))
            {
                if (modGroup.MaxSelections == 1)
                {
                    bool hasSelection = _allRadioButtons.Any(r => r.GroupName == modGroup.Name && r.IsChecked == true);
                    if (!hasSelection)
                    {
                        MessageBox.Show($"Debe seleccionar una opción de {modGroup.Name}.", "Selección Obligatoria", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    int selectedCount = _allCheckboxes.Count(c => ((ModifierOption)c.Tag).ProductModifierId == modGroup.Id && c.IsChecked == true);
                    if (selectedCount < modGroup.MinSelections)
                    {
                        MessageBox.Show($"Debe seleccionar al menos {modGroup.MinSelections} opciones de {modGroup.Name}.", "Selección Obligatoria", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            foreach (var rb in _allRadioButtons.Where(r => r.IsChecked == true))
            {
                var opt = (ModifierOption)rb.Tag;
                SelectedModifiers.Add(new {
                    Id = opt.Id,
                    Nombre = modGroup_Name(opt),
                    Seleccion = opt.Name,
                    CostoExtra = opt.PriceAdjustment
                });
            }

            foreach (var cb in _allCheckboxes.Where(c => c.IsChecked == true))
            {
                var opt = (ModifierOption)cb.Tag;
                SelectedModifiers.Add(new {
                    Id = opt.Id,
                    Nombre = modGroup_Name(opt),
                    Seleccion = opt.Name,
                    CostoExtra = opt.PriceAdjustment
                });
            }

            DialogResult = true;
            Close();
        }

        private string modGroup_Name(ModifierOption opt)
        {
            return _modifiers.FirstOrDefault(m => m.Id == opt.ProductModifierId)?.Name ?? "Opciones";
        }
    }
}
