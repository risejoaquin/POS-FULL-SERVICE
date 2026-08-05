with open('PosCore/Views/MainWindow.xaml', 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = lines[:88] + [
    '                    <Border BorderBrush="#115b30" BorderThickness="1" CornerRadius="20" Padding="5,5,15,5" Background="#115b30">\n',
    '                        <StackPanel Orientation="Horizontal">\n',
    '                            <Border Width="30" Height="30" CornerRadius="15" Background="#0d4722" Margin="0,0,10,0">\n',
    '                                <TextBlock Text="👤" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center" FontSize="14"/>\n',
    '                            </Border>\n',
    '                            <StackPanel VerticalAlignment="Center">\n',
    '                                <TextBlock Text="{Binding CurrentUsername}" Foreground="White" FontWeight="Bold" FontSize="12"/>\n',
    '                                <TextBlock Text="{Binding CurrentUserRole}" Foreground="#A7F3D0" FontWeight="SemiBold" FontSize="10"/>\n',
    '                            </StackPanel>\n',
    '                        </StackPanel>\n',
    '                    </Border>\n',
    '                </StackPanel>\n'
] + lines[104:]

with open('PosCore/Views/MainWindow.xaml', 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
