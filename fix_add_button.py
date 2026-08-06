with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

old_button = """                            <Button Grid.Column="2" Content="+" 
                                    Command="{Binding DataContext.AddToCartCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                   CommandParameter="{Binding}"
                                   Width="40" Height="40" Margin="10,0,0,0" 
                                   Background="{StaticResource PrimaryBrush}" Foreground="White" BorderThickness="0" Cursor="Hand" FontSize="20" FontWeight="Bold">
                                <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="8"/></Style></Button.Resources>
                            </Button>"""

new_button = """                            <Button Grid.Column="2" 
                                    Command="{Binding DataContext.AddToCartCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                   CommandParameter="{Binding}"
                                   Width="90" Height="40" Margin="10,0,0,0" 
                                   Background="{StaticResource PrimaryBrush}" Foreground="White" BorderThickness="0" Cursor="Hand" FontSize="14" FontWeight="Bold">
                                <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
                                    <TextBlock Text="+" Margin="0,0,5,0" FontWeight="Bold" FontSize="16" VerticalAlignment="Center"/>
                                    <TextBlock Text="Añadir" VerticalAlignment="Center"/>
                                </StackPanel>
                                <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="8"/></Style></Button.Resources>
                            </Button>"""

content = content.replace(old_button, new_button)

# Also fix the grid columns so there's enough space for Width="90"
content = content.replace('<ColumnDefinition Width="Auto"/>', '<ColumnDefinition Width="Auto"/>\n                            <ColumnDefinition Width="*"/>\n                            <ColumnDefinition Width="Auto"/>')
content = content.replace('<Grid.ColumnDefinitions>\n                                <ColumnDefinition Width="*"/>\n                                <ColumnDefinition Width="Auto"/>\n                                <ColumnDefinition Width="Auto"/>\n                            </Grid.ColumnDefinitions>', '<Grid.ColumnDefinitions>\n                                <ColumnDefinition Width="*"/>\n                                <ColumnDefinition Width="Auto"/>\n                            </Grid.ColumnDefinitions>')

with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
