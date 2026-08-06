import re

with open('PosBuilder/MainWindow.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

old_modules = """                    <!-- Modules Box -->
                    <Border Background="White" CornerRadius="12" Padding="15">
                        <StackPanel>
                            <TextBlock Text="Módulos" Foreground="#64748B" FontSize="12" FontWeight="SemiBold" Margin="0,0,0,10"/>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                                <Ellipse Width="12" Height="12" Fill="#10B981" Margin="0,0,8,0"/>
                                <TextBlock Text="Inventario" Foreground="#334155" FontSize="12"/>
                            </StackPanel>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                                <Ellipse Width="12" Height="12" Fill="#10B981" Margin="0,0,8,0"/>
                                <TextBlock Text="Reportes" Foreground="#334155" FontSize="12"/>
                            </StackPanel>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                                <Ellipse Width="12" Height="12" Fill="#E2E8F0" Margin="0,0,8,0"/>
                                <TextBlock Text="Crédito / Vales" Foreground="#94A3B8" FontSize="12"/>
                            </StackPanel>
                            <StackPanel Orientation="Horizontal">
                                <Ellipse Width="12" Height="12" Fill="#E2E8F0" Margin="0,0,8,0"/>
                                <TextBlock Text="Multi-Sucursal" Foreground="#94A3B8" FontSize="12"/>
                            </StackPanel>
                        </StackPanel>
                    </Border>"""

new_modules = """                    <!-- Modules Box -->
                    <Border Background="White" CornerRadius="12" Padding="15">
                        <Border.Resources>
                            <Style TargetType="Ellipse" x:Key="ModuleStatusIcon">
                                <Setter Property="Width" Value="12"/>
                                <Setter Property="Height" Value="12"/>
                                <Setter Property="Margin" Value="0,0,8,0"/>
                                <Setter Property="Fill" Value="#E2E8F0"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding RelativeSource={RelativeSource AncestorType=StackPanel}, Path=Tag}" Value="True">
                                        <Setter Property="Fill" Value="#10B981"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                            <Style TargetType="TextBlock" x:Key="ModuleStatusText">
                                <Setter Property="FontSize" Value="12"/>
                                <Setter Property="Foreground" Value="#94A3B8"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding RelativeSource={RelativeSource AncestorType=StackPanel}, Path=Tag}" Value="True">
                                        <Setter Property="Foreground" Value="#334155"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </Border.Resources>
                        <StackPanel>
                            <TextBlock Text="Módulos" Foreground="#64748B" FontSize="12" FontWeight="SemiBold" Margin="0,0,0,10"/>
                            
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,6" Tag="{Binding ModuleInventory}">
                                <Ellipse Style="{StaticResource ModuleStatusIcon}"/>
                                <TextBlock Text="Inventario" Style="{StaticResource ModuleStatusText}"/>
                            </StackPanel>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,6" Tag="{Binding ModuleReports}">
                                <Ellipse Style="{StaticResource ModuleStatusIcon}"/>
                                <TextBlock Text="Reportes" Style="{StaticResource ModuleStatusText}"/>
                            </StackPanel>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,6" Tag="{Binding ModuleCredit}">
                                <Ellipse Style="{StaticResource ModuleStatusIcon}"/>
                                <TextBlock Text="Crédito / Vales" Style="{StaticResource ModuleStatusText}"/>
                            </StackPanel>
                            <StackPanel Orientation="Horizontal" Tag="{Binding ModuleMultiStore}">
                                <Ellipse Style="{StaticResource ModuleStatusIcon}"/>
                                <TextBlock Text="Multi-Sucursal" Style="{StaticResource ModuleStatusText}"/>
                            </StackPanel>
                        </StackPanel>
                    </Border>"""

content = content.replace(old_modules, new_modules)
with open('PosBuilder/MainWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(content)

print("Modules patched.")
