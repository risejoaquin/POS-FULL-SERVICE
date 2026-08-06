import re

with open('./PosCore/Views/ManagerOverrideWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('PIN:', 'Contraseña:')
content = content.replace('PIN del', 'Contraseña del')

with open('./PosCore/Views/ManagerOverrideWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
