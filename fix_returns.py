import re

filepath = './PosCore/Views/ReturnsWindow.xaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

buttons_old = """                                <WrapPanel>
                                    <Button x:Name="BtnReturn" Content="Devolver Todo" 
                                            Command="{Binding DataContext.ReturnOrderCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                            CommandParameter="{Binding}"
                                            Padding="10,6" Background="{StaticResource ErrorBrush}" Foreground="White" BorderThickness="0" Cursor="Hand" HorizontalAlignment="Left" Margin="0,0,5,0"/>
                                    <Button x:Name="BtnPartialReturn" Content="Devolver Parcial" 
                                            Command="{Binding DataContext.PartialReturnOrderCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                            CommandParameter="{Binding}"
                                            Padding="10,6" Background="{StaticResource WarningBrush}" Foreground="White" BorderThickness="0" Cursor="Hand" HorizontalAlignment="Left" Margin="0,0,5,0"/>
                                        
                                    <Button x:Name="BtnReprint" Content="Ticket" 
                                            Command="{Binding DataContext.ReprintOrderCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                            CommandParameter="{Binding}"
                                            Padding="10,6" Background="{StaticResource PrimaryBrush}" Foreground="White" BorderThickness="0" Cursor="Hand" HorizontalAlignment="Left" Margin="0,0,5,0"/>
                                        
                                    <Button x:Name="BtnReprintReturn" Content="Nota de Crédito" 
                                            Command="{Binding DataContext.ReprintReturnCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                            CommandParameter="{Binding}"
                                            Padding="10,6" Background="{StaticResource AccentBrush}" Foreground="White" BorderThickness="0" Cursor="Hand" HorizontalAlignment="Left" Visibility="Collapsed"/>
                                </WrapPanel>

                                
                                <DataTemplate.Triggers>
                                    <DataTrigger Binding="{Binding IsReturned}" Value="True">
                                        <Setter TargetName="BtnReturn" Property="IsEnabled" Value="False"/>
                                        <Setter TargetName="BtnReturn" Property="Background" Value="#E5E7EB"/>
                                        <Setter TargetName="BtnReturn" Property="Foreground" Value="#9CA3AF"/>
                                        <Setter TargetName="BtnPartialReturn" Property="IsEnabled" Value="False"/>
                                        <Setter TargetName="BtnPartialReturn" Property="Background" Value="#E5E7EB"/>
                                        <Setter TargetName="BtnPartialReturn" Property="Foreground" Value="#9CA3AF"/>
                                        <Setter TargetName="BtnReprintReturn" Property="Visibility" Value="Visible"/>
                                    </DataTrigger>
                                </DataTemplate.Triggers>"""

buttons_new = """                                <WrapPanel>
                                    <Button x:Name="BtnReturn" Content="↩️ Devolver Todo" 
                                            Command="{Binding DataContext.ReturnOrderCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                            CommandParameter="{Binding}"
                                            Padding="10,6" Background="#FEF2F2" Foreground="#991B1B" BorderBrush="#FECACA" BorderThickness="1" Cursor="Hand" HorizontalAlignment="Left" Margin="0,0,5,0">
                                        <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="6"/></Style></Button.Resources>
                                    </Button>
                                    <Button x:Name="BtnPartialReturn" Content="🔄 Parcial" 
                                            Command="{Binding DataContext.PartialReturnOrderCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                            CommandParameter="{Binding}"
                                            Padding="10,6" Background="#FFFBEB" Foreground="#B45309" BorderBrush="#FDE68A" BorderThickness="1" Cursor="Hand" HorizontalAlignment="Left" Margin="0,0,5,0">
                                        <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="6"/></Style></Button.Resources>
                                    </Button>
                                    <Button x:Name="BtnReprint" Content="🖨️ Ticket" 
                                            Command="{Binding DataContext.ReprintOrderCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                            CommandParameter="{Binding}"
                                            Padding="10,6" Background="#EFF6FF" Foreground="#1D4ED8" BorderBrush="#BFDBFE" BorderThickness="1" Cursor="Hand" HorizontalAlignment="Left" Margin="0,0,5,0">
                                        <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="6"/></Style></Button.Resources>
                                    </Button>
                                    <Button x:Name="BtnReprintReturn" Content="📄 N. Crédito" 
                                            Command="{Binding DataContext.ReprintReturnCommand, RelativeSource={RelativeSource AncestorType=Window}}" 
                                            CommandParameter="{Binding}"
                                            Padding="10,6" Background="#FAF5FF" Foreground="#7E22CE" BorderBrush="#E9D5FF" BorderThickness="1" Cursor="Hand" HorizontalAlignment="Left" Visibility="Collapsed">
                                        <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="6"/></Style></Button.Resources>
                                    </Button>
                                </WrapPanel>

                                
                                <DataTemplate.Triggers>
                                    <DataTrigger Binding="{Binding IsReturned}" Value="True">
                                        <Setter TargetName="BtnReturn" Property="IsEnabled" Value="False"/>
                                        <Setter TargetName="BtnReturn" Property="Background" Value="#F3F4F6"/>
                                        <Setter TargetName="BtnReturn" Property="Foreground" Value="#9CA3AF"/>
                                        <Setter TargetName="BtnReturn" Property="BorderBrush" Value="#E5E7EB"/>
                                        <Setter TargetName="BtnPartialReturn" Property="IsEnabled" Value="False"/>
                                        <Setter TargetName="BtnPartialReturn" Property="Background" Value="#F3F4F6"/>
                                        <Setter TargetName="BtnPartialReturn" Property="Foreground" Value="#9CA3AF"/>
                                        <Setter TargetName="BtnPartialReturn" Property="BorderBrush" Value="#E5E7EB"/>
                                        <Setter TargetName="BtnReprintReturn" Property="Visibility" Value="Visible"/>
                                    </DataTrigger>
                                </DataTemplate.Triggers>"""

content = content.replace(buttons_old, buttons_new)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated ReturnsWindow.xaml")
