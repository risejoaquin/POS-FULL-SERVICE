import re

with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

# Make sure SearchBox has the name SearchBox
if 'x:Name="SearchBox"' not in content:
    content = content.replace('<TextBox Text="{Binding SearchQuery', '<TextBox x:Name="SearchBox" Text="{Binding SearchQuery')

with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
