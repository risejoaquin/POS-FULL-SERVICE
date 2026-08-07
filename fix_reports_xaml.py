with open('./PosCore/Views/ReportsWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('Padding="15,0" Padding="5,8"', 'Padding="15,0"')

with open('./PosCore/Views/ReportsWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
