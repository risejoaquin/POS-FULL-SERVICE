import re

with open('./PosCore/Views/ReturnsWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

# Change "Actualizar lista" to something that shows NotificationMessage
if 'NotificationMessage' not in content:
    content = content.replace('Content="Actualizar lista"', 'Content="Actualizar lista" Margin="0,0,10,0"')
    content = content.replace('</StackPanel>', '    <TextBlock Text="{Binding NotificationMessage}" Foreground="{StaticResource SuccessBrush}" VerticalAlignment="Center" Margin="10,0,0,0"/>\n        </StackPanel>')

with open('./PosCore/Views/ReturnsWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)

with open('./PosCore/Views/PartialReturnWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('PIN del Administrador', 'Contraseña del Administrador')
content = content.replace('PIN:', 'Contraseña:')
content = content.replace('PasswordChanged="AdminPin_PasswordChanged"', 'PasswordChanged="AdminPin_PasswordChanged"') # Leave this as is

with open('./PosCore/Views/PartialReturnWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
