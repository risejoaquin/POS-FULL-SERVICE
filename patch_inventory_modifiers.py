import re

with open('./PosCore/Views/InventoryWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

old_buttons = """                    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                        <Button Content="Cancelar" Command="{Binding CancelEditCommand}" Width="100" Height="40" Background="{StaticResource BorderBrush}" Foreground="{StaticResource TextPrimaryBrush}" FontWeight="Bold" BorderThickness="0" Margin="0,0,10,0" Cursor="Hand"/>
                        <Button Content="Guardar" Command="{Binding SaveProductCommand}" Width="120" Height="40" Background="{StaticResource SuccessBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand"/>
                    </StackPanel>"""

new_buttons = """                    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                        <Button Content="Modificadores" Command="{Binding ConfigureModifiersCommand}" Width="120" Height="40" Background="{StaticResource PrimaryBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Margin="0,0,10,0" Cursor="Hand" Visibility="{Binding IsEditingNew, Converter={StaticResource InverseBooleanToVisibilityConverter}, FallbackValue=Collapsed}"/>
                        <Button Content="Cancelar" Command="{Binding CancelEditCommand}" Width="100" Height="40" Background="{StaticResource BorderBrush}" Foreground="{StaticResource TextPrimaryBrush}" FontWeight="Bold" BorderThickness="0" Margin="0,0,10,0" Cursor="Hand"/>
                        <Button Content="Guardar" Command="{Binding SaveProductCommand}" Width="120" Height="40" Background="{StaticResource SuccessBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand"/>
                    </StackPanel>"""

content = content.replace(old_buttons, new_buttons)

with open('./PosCore/Views/InventoryWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
