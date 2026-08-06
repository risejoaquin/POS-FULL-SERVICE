import re

with open('./PosCore/Views/InventoryWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

new_fields = """                    <TextBlock Text="Variantes / Modificadores Rápidos (Separados por coma)" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,10,0,5"/>
                    <TextBox Text="{Binding Variantes, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,0,15" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>

                    <TextBlock Text="Alérgenos o Notas de Cocina" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,10,0,5"/>
                    <TextBox Text="{Binding NotasCocina, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,0,15" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>
"""

content = re.sub(
    r'(<Button\s+Content="Guardar Producto")',
    new_fields + r'\1',
    content
)

with open('./PosCore/Views/InventoryWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
