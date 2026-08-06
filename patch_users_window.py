import re

with open('./PosCore/Views/UsersWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('Header="PIN"', 'Header="Contraseña"')
content = content.replace('Binding="{Binding Pin}"', 'Binding="{Binding Pin}"')
content = content.replace('PIN / Contraseña', 'Contraseña')
content = content.replace('Text="PIN:"', 'Text="Contraseña:"')
content = content.replace('Header="Reset PIN"', 'Header="Reset Contraseña"')

with open('./PosCore/Views/UsersWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
