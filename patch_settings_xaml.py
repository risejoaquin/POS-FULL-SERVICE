with open('./PosCore/Views/SettingsWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

old_columns = """            <DataGrid.Columns>
                <DataGridTextColumn Header="Nombre" Binding="{Binding Name}" Width="130" />
                <DataGridTextColumn Header="Ícono" Binding="{Binding Icon}" Width="60" />
                <DataGridTextColumn Header="Color (Hex)" Binding="{Binding Color}" Width="90" />
                <DataGridTextColumn Header="Acción" Binding="{Binding Action}" Width="120" />
                <DataGridTextColumn Header="Descripción" Binding="{Binding Description}" Width="*" />
            </DataGrid.Columns>"""

new_columns = """            <DataGrid.Columns>
                <DataGridTextColumn Header="Nombre" Binding="{Binding Name, UpdateSourceTrigger=PropertyChanged}" Width="130" />
                <DataGridTextColumn Header="Ícono" Binding="{Binding Icon, UpdateSourceTrigger=PropertyChanged}" Width="60" />
                <DataGridTextColumn Header="Color (Hex)" Binding="{Binding Color, UpdateSourceTrigger=PropertyChanged}" Width="90" />
                <DataGridComboBoxColumn Header="Acción" SelectedItemBinding="{Binding Action, UpdateSourceTrigger=PropertyChanged}" Width="140">
                    <DataGridComboBoxColumn.ElementStyle>
                        <Style TargetType="ComboBox">
                            <Setter Property="ItemsSource" Value="{Binding AvailableActions, RelativeSource={RelativeSource AncestorType=Window}}" />
                        </Style>
                    </DataGridComboBoxColumn.ElementStyle>
                    <DataGridComboBoxColumn.EditingElementStyle>
                        <Style TargetType="ComboBox">
                            <Setter Property="ItemsSource" Value="{Binding AvailableActions, RelativeSource={RelativeSource AncestorType=Window}}" />
                        </Style>
                    </DataGridComboBoxColumn.EditingElementStyle>
                </DataGridComboBoxColumn>
                <DataGridTextColumn Header="Descripción" Binding="{Binding Description, UpdateSourceTrigger=PropertyChanged}" Width="*" />
            </DataGrid.Columns>"""

content = content.replace(old_columns, new_columns)

with open('./PosCore/Views/SettingsWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
