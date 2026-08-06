with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
'''                            <Button Grid.Column="2" Content="Añadir +" Width="100" 
                                    Command="{Binding DataContext.AddToCartCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                   CommandParameter="{Binding}"
                                   Width="40" Height="40" Margin="10,0,0,0" ''',
'''                            <Button Grid.Column="2" Content="Añadir +" 
                                    Command="{Binding DataContext.AddToCartCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                   CommandParameter="{Binding}"
                                   Width="100" Height="40" Margin="10,0,0,0" '''
)

with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
