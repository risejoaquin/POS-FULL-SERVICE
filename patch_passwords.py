import re

with open('./PosCore/Views/UsersWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace NewPin TextBox with PasswordBox and hook PasswordChanged
content = content.replace('<TextBox Text="{Binding NewPin, UpdateSourceTrigger=PropertyChanged}" Padding="5" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1"/>', '<PasswordBox x:Name="NewPinBox" PasswordChanged="NewPinBox_PasswordChanged" Padding="5" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1"/>')

with open('./PosCore/Views/UsersWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)

with open('./PosCore/Views/UsersWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content_cs = f.read()

if 'NewPinBox_PasswordChanged' not in content_cs:
    handler = """
    private void NewPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is PosCore.ViewModels.UsersViewModel vm)
        {
            if (sender is System.Windows.Controls.PasswordBox pb)
            {
                vm.NewPin = pb.Password;
            }
        }
    }
"""
    idx = content_cs.rfind('}')
    idx = content_cs.rfind('}', 0, idx)
    content_cs = content_cs[:idx] + handler + content_cs[idx:]

with open('./PosCore/Views/UsersWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content_cs)
