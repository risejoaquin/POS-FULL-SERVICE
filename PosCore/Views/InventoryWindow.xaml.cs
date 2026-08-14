using System.Windows;
using PosCore.ViewModels;

namespace PosCore.Views;

public partial class InventoryWindow : Window
{
    public InventoryWindow(InventoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox tb)
            tb.SelectAll();
    }
}
