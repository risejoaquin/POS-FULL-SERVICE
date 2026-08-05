import re
with open('PosCore/Views/MainWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

resources = """
    <Window.Resources>
        <BooleanToVisibilityConverter x:Key="BoolToVis"/>
    </Window.Resources>
"""
content = re.sub(r'<Window.Resources>\s*</Window.Resources>', resources, content)

# Top Right (Clock & User)
user_patch = """
                    <Border BorderBrush="#115b30" BorderThickness="1" CornerRadius="20" Padding="5,5,15,5" Background="#115b30">
                        <StackPanel Orientation="Horizontal">
                            <Border Width="30" Height="30" CornerRadius="15" Background="#0d4722" Margin="0,0,10,0">
                                <TextBlock Text="👤" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center" FontSize="14"/>
                            </Border>
                            <StackPanel VerticalAlignment="Center">
                                <TextBlock Text="{Binding CurrentUsername}" Foreground="White" FontWeight="Bold" FontSize="12"/>
                                <TextBlock Text="{Binding CurrentUserRole}" Foreground="#A7F3D0" FontWeight="SemiBold" FontSize="10"/>
                            </StackPanel>
                        </StackPanel>
                    </Border>
"""
content = re.sub(r'<Border BorderBrush="#115b30".*?</Border>', user_patch, content, flags=re.DOTALL)


# Bottom menu
bottom_menu_patch = """
                <!-- Bottom Menu -->
                <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
                    <Button Content="Inventario" Command="{Binding OpenInventoryCommand}" Visibility="{Binding IsAdmin, Converter={StaticResource BoolToVis}}" Foreground="White" Background="Transparent" BorderThickness="0" Margin="0,0,15,0" FontWeight="SemiBold" FontSize="12" Cursor="Hand"/>
                    <Button Content="Usuarios" Command="{Binding OpenUsersCommand}" Visibility="{Binding IsAdmin, Converter={StaticResource BoolToVis}}" Foreground="White" Background="Transparent" BorderThickness="0" Margin="0,0,15,0" FontWeight="SemiBold" FontSize="12" Cursor="Hand"/>
                    <Button Content="Reportes" Command="{Binding OpenReportsCommand}" Visibility="{Binding IsAdmin, Converter={StaticResource BoolToVis}}" Foreground="White" Background="Transparent" BorderThickness="0" Margin="0,0,15,0" FontWeight="SemiBold" FontSize="12" Cursor="Hand"/>
                    <Button Content="Devoluciones" Command="{Binding OpenReturnsCommand}" Visibility="{Binding IsAdmin, Converter={StaticResource BoolToVis}}" Foreground="White" Background="Transparent" BorderThickness="0" Margin="0,0,15,0" FontWeight="SemiBold" FontSize="12" Cursor="Hand"/>
                    <Button Content="Config" Command="{Binding OpenSettingsCommand}" Visibility="{Binding IsAdmin, Converter={StaticResource BoolToVis}}" Foreground="White" Background="Transparent" BorderThickness="0" Margin="0,0,15,0" FontWeight="SemiBold" FontSize="12" Cursor="Hand"/>
                </StackPanel>
"""
content = re.sub(r'<!-- Bottom Menu -->.*?</StackPanel>', bottom_menu_patch, content, flags=re.DOTALL)

with open('PosCore/Views/MainWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
print("MainWindow patched")
