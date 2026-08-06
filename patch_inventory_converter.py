import re

with open('./PosCore/Views/InventoryWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(' Visibility="{Binding IsEditingNew, Converter={StaticResource InverseBooleanToVisibilityConverter}, FallbackValue=Collapsed}"', '')

with open('./PosCore/Views/InventoryWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
