import re

with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

old_button = """<Button Grid.Row="2" Content="Agregar" Command="{Binding DataContext.AddToCartCommand, RelativeSource={RelativeSource AncestorType=UserControl}}" CommandParameter="{Binding}" Margin="0,5,0,0"/>"""
new_button = """<Button Grid.Row="2" Command="{Binding DataContext.AddToCartCommand, RelativeSource={RelativeSource AncestorType=UserControl}}" CommandParameter="{Binding}" Margin="0,5,0,0" Padding="5">
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
        <TextBlock Text="➕" Margin="0,0,5,0" />
        <TextBlock Text="Agregar" />
    </StackPanel>
</Button>"""

content = content.replace(old_button, new_button)

# Also add F1 hotkey to SearchBox if it's there
# We'll just trust that MainWindow PreviewKeyDown handles F1 to FocusSearchCommand. We need to implement FocusSearchCommand in MainViewModel.

with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
