import re

with open('./PosCore/Views/MainWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Make sure we subscribe to OnFocusSearchRequested
hook_code = """
        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.OnFocusSearchRequested += () => 
            {
                // We need to find the SearchBox inside ProductsPanel
                var searchBox = ProductsPanel.FindName("SearchBox") as System.Windows.Controls.TextBox;
                if (searchBox != null)
                {
                    searchBox.Focus();
                    searchBox.SelectAll();
                }
            };
        }
"""
if 'OnFocusSearchRequested' not in content:
    content = content.replace('InitializeComponent();', 'InitializeComponent();' + hook_code)

with open('./PosCore/Views/MainWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
