import re

with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix Grid.ColumnDefinitions
content = content.replace('''                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>''', '''                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>''')

content = content.replace('''                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="60"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>''', '''                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="60"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>''')

content = content.replace('<Button Grid.Column="2" Content="Añadir +" Width="100"', '<Button Grid.Column="2" Content="Añadir +"')

with open('./PosCore/Views/Controls/ProductsPanelControl.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
