with open('./PosCore/Views/ReturnsWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
    '<Button Grid.Column="1" Command="{Binding LoadOrdersCommand}" Content="Actualizar Lista" Padding="15,8" Background="{StaticResource PrimaryBrush}" Foreground="White" BorderThickness="0" Cursor="Hand"/>',
    '<StackPanel Grid.Column="1" Orientation="Horizontal" HorizontalAlignment="Right"><TextBlock Text="{Binding NotificationMessage}" Foreground="{StaticResource SuccessBrush}" VerticalAlignment="Center" Margin="0,0,10,0" FontWeight="SemiBold"/><Button Command="{Binding LoadOrdersCommand}" Content="Actualizar Lista" Padding="15,8" Background="{StaticResource PrimaryBrush}" Foreground="White" BorderThickness="0" Cursor="Hand"/></StackPanel>'
)

with open('./PosCore/Views/ReturnsWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
