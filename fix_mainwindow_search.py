with open('./PosCore/Views/MainWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("            if (Keyboard.Modifiers == ModifierKeys.Alt)", "            if (e.Key == Key.F1)\n            {\n                ProductsPanel.FocusSearch();\n                e.Handled = true;\n                return;\n            }\n            if (Keyboard.Modifiers == ModifierKeys.Alt)")

with open('./PosCore/Views/MainWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
