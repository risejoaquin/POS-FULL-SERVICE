content = """using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PosBuilder.Views.Controls
{
    public partial class ColorPickerControl : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                "SelectedColor", 
                typeof(string), 
                typeof(ColorPickerControl), 
                new FrameworkPropertyMetadata("#2563EB", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        public string SelectedColor
        {
            get => (string)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public event EventHandler<string> ColorChanged;

        public ObservableCollection<PaletteColor> Palette { get; set; } = new ObservableCollection<PaletteColor>();
        private bool _isUpdatingFromCode = false;

        public ColorPickerControl()
        {
            InitializeComponent();
            
            // Selected colors matching the mockup
            string[] hexColors = { 
                "#2563EB", "#9333EA", "#10B981", "#F59E0B", "#14B8A6", "#EC4899", "#3B82F6", "#6366F1"
            };

            foreach (var hex in hexColors)
            {
                Palette.Add(new PaletteColor { Hex = hex, Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)) });
            }

            PaletteItems.ItemsSource = Palette;
            UpdateColorUI(SelectedColor ?? "#2563EB");
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPickerControl control && e.NewValue is string hex)
            {
                control.UpdateColorUI(hex);
            }
        }

        private void HexInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromCode) return;

            string hex = HexInput.Text;
            if (IsValidHex(hex))
            {
                SelectedColor = hex;
                ColorPreview.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                ColorChanged?.Invoke(this, hex);
            }
        }

        private void PaletteColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PaletteColor pc)
            {
                SelectedColor = pc.Hex;
                ColorChanged?.Invoke(this, pc.Hex);
            }
        }

        private void OpenColorPicker_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            
            if (IsValidHex(SelectedColor))
            {
                var color = (Color)ColorConverter.ConvertFromString(SelectedColor);
                colorDialog.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string hex = $"#{colorDialog.Color.R:X2}{colorDialog.Color.G:X2}{colorDialog.Color.B:X2}";
                SelectedColor = hex;
                ColorChanged?.Invoke(this, hex);
            }
        }

        private void UpdateColorUI(string hex)
        {
            if (IsValidHex(hex))
            {
                _isUpdatingFromCode = true;
                if (HexInput != null && HexInput.Text != hex) 
                    HexInput.Text = hex;
                if (ColorPreview != null) 
                    ColorPreview.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                _isUpdatingFromCode = false;
            }
        }

        private bool IsValidHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return false;
            return Regex.IsMatch(hex, "^#(?:[0-9a-fA-F]{3}){1,2}$");
        }
    }

    public class PaletteColor
    {
        public string Hex { get; set; }
        public SolidColorBrush Brush { get; set; }
    }
}
"""
with open('PosBuilder/Views/Controls/ColorPickerControl.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
