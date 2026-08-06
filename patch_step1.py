with open('PosBuilder/Views/Step1Environment.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

old_cb = """        <ComboBox ToolTip="Selecciona el tipo de negocio para generar la base de datos adecuada." Text="{Binding BusinessType, UpdateSourceTrigger=PropertyChanged}" Padding="8" Margin="0,0,0,15" FontSize="14">
            <ComboBoxItem Content="Retail"/>
            <ComboBoxItem Content="Hospitality"/>
            <ComboBoxItem Content="Services"/>
        </ComboBox>"""

new_cb = """        <ComboBox ToolTip="Selecciona el tipo de negocio para generar la base de datos adecuada." Text="{Binding BusinessType, UpdateSourceTrigger=PropertyChanged}" Padding="8" Margin="0,0,0,15" FontSize="14">
            <ComboBoxItem Content="Abarrotes / Minimarket"/>
            <ComboBoxItem Content="Cafetería"/>
            <ComboBoxItem Content="Ferretería"/>
            <ComboBoxItem Content="Restaurante / Comida Rápida"/>
            <ComboBoxItem Content="Boutique / Ropa"/>
            <ComboBoxItem Content="Farmacia"/>
            <ComboBoxItem Content="Papelería"/>
            <ComboBoxItem Content="Zapatería"/>
            <ComboBoxItem Content="Electrónica / Tecnología"/>
            <ComboBoxItem Content="Peluquería / Barbería"/>
            <ComboBoxItem Content="Taller Mecánico"/>
            <ComboBoxItem Content="Servicios Profesionales"/>
            <ComboBoxItem Content="Otro"/>
        </ComboBox>"""

content = content.replace(old_cb, new_cb)

with open('PosBuilder/Views/Step1Environment.xaml', 'w', encoding='utf-8') as f:
    f.write(content)

print("Step1 updated")
