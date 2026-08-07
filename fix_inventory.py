filepath = './PosCore/Views/InventoryWindow.xaml'
with open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_content = """                    <StackPanel Margin="0,0,10,0">
                    <TextBlock Text="Detalles del Producto" FontSize="20" FontWeight="Bold" Foreground="{StaticResource TextPrimaryBrush}" Margin="0,0,0,20"/>
                        
                    <!-- Información Básica -->
                    <Border Background="#F8FAFC" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" CornerRadius="8" Padding="15" Margin="0,0,0,15">
                        <StackPanel>
                            <TextBlock Text="Información Básica" FontSize="16" FontWeight="Bold" Foreground="{StaticResource PrimaryBrush}" Margin="0,0,0,10"/>
                            
                            <TextBlock Text="Código de Barras" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,0,0,5"/>
                            <Grid Margin="0,0,0,15">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                <TextBox Text="{Binding EditingProduct.Barcode, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,10,0" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>
                                <Button Grid.Column="1" Content="Generar" Command="{Binding GenerateBarcodeCommand}" MinWidth="80" Padding="15,0" Height="40" Background="{StaticResource PrimaryBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand" ToolTip="Generar Código Automático">
                                    <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="6"/></Style></Button.Resources>
                                </Button>
                            </Grid>

                            <TextBlock Text="Nombre del Producto" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,0,0,5"/>
                            <TextBox Text="{Binding EditingProduct.Name, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,0,0" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>
                        </StackPanel>
                    </Border>

                    <!-- Precios y Stock -->
                    <Border Background="#F8FAFC" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" CornerRadius="8" Padding="15" Margin="0,0,0,15">
                        <StackPanel>
                            <TextBlock Text="Precios y Stock" FontSize="16" FontWeight="Bold" Foreground="{StaticResource PrimaryBrush}" Margin="0,0,0,10"/>
                            
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="10"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>
                                <StackPanel Grid.Column="0">
                                    <TextBlock Text="Precio (Venta)" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,0,0,5"/>
                                    <TextBox Text="{Binding EditingProduct.Price, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,0,15" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>
                                </StackPanel>
                                <StackPanel Grid.Column="2">
                                    <TextBlock Text="Stock Actual" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,0,0,5"/>
                                    <TextBox Text="{Binding EditingProduct.StockQuantity, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,0,15" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>
                                </StackPanel>
                            </Grid>
                            
                            <TextBlock Text="Alerta Stock Mínimo" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,0,0,5"/>
                            <TextBox Text="{Binding EditingProduct.MinStockThreshold, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,0,0" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>
                        </StackPanel>
                    </Border>
                    
                    <!-- Configuración Extra -->
                    <Border Background="#F8FAFC" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" CornerRadius="8" Padding="15" Margin="0,0,0,25">
                        <StackPanel>
                            <TextBlock Text="Configuración Extra" FontSize="16" FontWeight="Bold" Foreground="{StaticResource PrimaryBrush}" Margin="0,0,0,10"/>

                            <TextBlock Text="Variantes / Modificadores Rápidos" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,0,0,5"/>
                            <TextBox Text="{Binding Variantes, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,0,15" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>

                            <TextBlock Text="Alérgenos o Notas de Cocina" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,0,0,5"/>
                            <TextBox Text="{Binding NotasCocina, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,0,15" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>

                            <TextBlock Text="URL de Imagen" FontSize="14" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}" Margin="0,0,0,5"/>
                            <TextBox Text="{Binding EditingProduct.ImagePath, UpdateSourceTrigger=PropertyChanged}" Height="40" Margin="0,0,0,0" Padding="10,0" FontSize="14" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Background="{StaticResource BackgroundBrush}" VerticalContentAlignment="Center" GotFocus="TextBox_GotFocus"/>
                        </StackPanel>
                    </Border>

                    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,0,0,10">
                        <Button Content="Cancelar" Command="{Binding CancelEditCommand}" MinWidth="100" Padding="15,0" Height="40" Background="{StaticResource BorderBrush}" Foreground="{StaticResource TextPrimaryBrush}" FontWeight="Bold" BorderThickness="0" Margin="0,0,10,0" Cursor="Hand">
                            <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="6"/></Style></Button.Resources>
                        </Button>
                        <Button Content="Guardar" Command="{Binding SaveProductCommand}" MinWidth="120" Padding="15,0" Height="40" Background="{StaticResource SuccessBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand">
                            <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="6"/></Style></Button.Resources>
                        </Button>
                    </StackPanel>
                </StackPanel>
"""

# Replace lines 81 to 121 (which corresponds to index 81 to 121 inclusive in python list if 0-based is 1-based minus 1.
# Actually, lines 82 to 122 in the cat -n output. That is index 81 to 121.

lines = lines[:81] + [new_content] + lines[122:]

with open(filepath, 'w', encoding='utf-8') as f:
    f.writelines(lines)
