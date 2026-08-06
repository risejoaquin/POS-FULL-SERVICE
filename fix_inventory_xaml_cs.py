import re

with open('./PosCore/Views/InventoryWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the inner function
content = content.replace(
'''    public InventoryWindow(InventoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
                tb.SelectAll();
        }
    }''',
'''    public InventoryWindow(InventoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox tb)
            tb.SelectAll();
    }'''
)

with open('./PosCore/Views/InventoryWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
