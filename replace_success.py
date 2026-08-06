content = """<Window x:Class="PosBuilder.Views.SuccessModal"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Generación Completada" Height="800" Width="1300"
        WindowStartupLocation="CenterOwner" Background="#0B1121"
        FontFamily="Segoe UI" WindowStyle="None" ResizeMode="NoResize"
        AllowsTransparency="False">
    <Grid>
        <!-- Center Content -->
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
            
            <!-- Checkmark icon box -->
            <Border Width="80" Height="80" Background="#2563EB" CornerRadius="16" HorizontalAlignment="Center" Margin="0,0,0,25">
                <Path Data="M 22,44 L 35,57 L 58,28" Stroke="White" StrokeThickness="4" StrokeStartLineCap="Round" StrokeEndLineCap="Round" StrokeLineJoin="Round" HorizontalAlignment="Center" VerticalAlignment="Center" Stretch="None"/>
            </Border>
            
            <TextBlock Text="Mi POS generado" FontSize="36" FontWeight="Bold" Foreground="White" HorizontalAlignment="Center" Margin="0,0,0,15"/>
            
            <TextBlock Text="Tu configuración POS ha sido compilada exitosamente." FontSize="16" Foreground="#94A3B8" HorizontalAlignment="Center" Margin="0,0,0,5"/>
            <TextBlock Text="Revisa el directorio de instalación." FontSize="16" Foreground="#94A3B8" HorizontalAlignment="Center" Margin="0,0,0,30"/>
            
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
                <Button Content="Volver al Resumen" Click="Close_Click" Width="160" Height="45" Background="Transparent" Foreground="White" BorderBrush="#334155" BorderThickness="1" Margin="0,0,15,0" Cursor="Hand" FontWeight="SemiBold">
                    <Button.Template>
                        <ControlTemplate TargetType="Button">
                            <Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="8">
                                <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            </Border>
                        </ControlTemplate>
                    </Button.Template>
                </Button>
                
                <Button Content="Nueva Configuración" Click="OpenFolder_Click" Width="180" Height="45" Background="#2563EB" Foreground="White" BorderThickness="0" Cursor="Hand" FontWeight="SemiBold">
                    <Button.Template>
                        <ControlTemplate TargetType="Button">
                            <Border Background="{TemplateBinding Background}" CornerRadius="8">
                                <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            </Border>
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            
        </StackPanel>
    </Grid>
</Window>
"""
with open('PosBuilder/Views/SuccessModal.xaml', 'w', encoding='utf-8') as f:
    f.write(content)

print("SuccessModal replaced.")
