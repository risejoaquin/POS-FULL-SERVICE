with open('PosBuilder/Views/Step4Branding.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

old_content = """<UserControl x:Class="PosBuilder.Views.Step4Branding"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="clr-namespace:PosBuilder.Views.Controls">
    <StackPanel Margin="20">
        <TextBlock Text="Nombre del POS" FontWeight="SemiBold" Margin="0,0,0,5"/>
        <TextBox ToolTip="Nombre de la marca o empresa que aparecerá en el POS." Text="{Binding BrandingName, UpdateSourceTrigger=PropertyChanged}" Padding="8" Margin="0,0,0,15" />
        
        <TextBlock Text="Color Principal (Hex)" FontWeight="SemiBold" Margin="0,0,0,5"/>
        <controls:ColorPickerControl ToolTip="Color principal que dominará la interfaz del POS." SelectedColor="{Binding BrandingColor, UpdateSourceTrigger=PropertyChanged}" Margin="0,0,0,15" />
        
        <TextBlock Text="Logo (Drag &amp; Drop)" FontWeight="SemiBold" Margin="0,0,0,5"/>
        <controls:FileBrowserControl ToolTip="Logotipo de la empresa (PNG o JPG recomendado)." FilePath="{Binding BrandingLogoPath, UpdateSourceTrigger=PropertyChanged}" Margin="0,0,0,15" />
    </StackPanel>
</UserControl>"""

new_content = """<UserControl x:Class="PosBuilder.Views.Step4Branding"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="clr-namespace:PosBuilder.Views.Controls">
    <StackPanel Margin="20">
        <TextBlock Text="NOMBRE DEL POS" Foreground="#64748B" FontSize="11" FontWeight="Bold" Margin="0,0,0,5"/>
        <TextBox ToolTip="Nombre de la marca o empresa que aparecerá en el POS." Text="{Binding BrandingName, UpdateSourceTrigger=PropertyChanged}" Padding="15,12" Margin="0,0,0,25" FontSize="14" BorderBrush="#E2E8F0" BorderThickness="1">
            <TextBox.Resources>
                <Style TargetType="Border">
                    <Setter Property="CornerRadius" Value="6"/>
                </Style>
            </TextBox.Resources>
        </TextBox>
        
        <TextBlock Text="COLOR PRINCIPAL (HEX)" Foreground="#64748B" FontSize="11" FontWeight="Bold" Margin="0,0,0,5"/>
        <controls:ColorPickerControl ToolTip="Color principal que dominará la interfaz del POS." SelectedColor="{Binding BrandingColor, UpdateSourceTrigger=PropertyChanged}" Margin="0,0,0,25" />
        
        <TextBlock Text="LOGO DEL POS" Foreground="#64748B" FontSize="11" FontWeight="Bold" Margin="0,0,0,5"/>
        <controls:FileBrowserControl ToolTip="Logotipo de la empresa (PNG o JPG recomendado)." FilePath="{Binding BrandingLogoPath, UpdateSourceTrigger=PropertyChanged}" Margin="0,0,0,15" />
    </StackPanel>
</UserControl>"""

content = content.replace(old_content, new_content)
with open('PosBuilder/Views/Step4Branding.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
print("Step4Branding.xaml updated")
