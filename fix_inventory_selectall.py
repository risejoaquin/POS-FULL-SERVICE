import re

with open('./PosCore/Views/InventoryWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
    'VerticalContentAlignment="Center"/>',
    'VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>'
)

with open('./PosCore/Views/InventoryWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)

with open('./PosCore/Views/InventoryWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

if "TextBox_GotFocus" not in content:
    content = content.replace(
        "    }\n}",
        "        private void TextBox_GotFocus(object sender, RoutedEventArgs e)\n        {\n            if (sender is System.Windows.Controls.TextBox tb)\n                tb.SelectAll();\n        }\n    }\n}"
    )
    with open('./PosCore/Views/InventoryWindow.xaml.cs', 'w', encoding='utf-8') as f:
        f.write(content)
