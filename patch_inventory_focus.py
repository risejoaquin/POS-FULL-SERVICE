import re

with open('./PosCore/Views/InventoryWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

# Add GotFocus event to TextBoxes
content = content.replace('<TextBox Text="{Binding EditingProduct.Price}"', '<TextBox Text="{Binding EditingProduct.Price}" GotFocus="TextBox_GotFocus"')
content = content.replace('<TextBox Text="{Binding EditingProduct.StockQuantity}"', '<TextBox Text="{Binding EditingProduct.StockQuantity}" GotFocus="TextBox_GotFocus"')
content = content.replace('<TextBox Text="{Binding EditingProduct.MinStockThreshold}"', '<TextBox Text="{Binding EditingProduct.MinStockThreshold}" GotFocus="TextBox_GotFocus"')

with open('./PosCore/Views/InventoryWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)

with open('./PosCore/Views/InventoryWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content_cs = f.read()

if 'TextBox_GotFocus' not in content_cs:
    handler = """
    private void TextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox tb)
        {
            tb.Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
        }
    }
"""
    # Insert before the last brace
    idx = content_cs.rfind('}')
    idx = content_cs.rfind('}', 0, idx)
    content_cs = content_cs[:idx] + handler + content_cs[idx:]

with open('./PosCore/Views/InventoryWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content_cs)
