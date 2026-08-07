import re

filepath = './PosCore/Views/ReportsWindow.xaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Update Filters
filters_old = """            <StackPanel Grid.Column="2" Orientation="Horizontal" VerticalAlignment="Center">
                <TextBlock Text="Desde:" VerticalAlignment="Center" Margin="0,0,5,0" Foreground="{StaticResource TextSecondaryBrush}"/>
                <DatePicker SelectedDate="{Binding StartDate}" Width="110" Margin="0,0,15,0" Background="{StaticResource SurfaceBrush}"/>
                
                <TextBlock Text="Hasta:" VerticalAlignment="Center" Margin="0,0,5,0" Foreground="{StaticResource TextSecondaryBrush}"/>
                <DatePicker SelectedDate="{Binding EndDate}" Width="110" Margin="0,0,15,0" Background="{StaticResource SurfaceBrush}"/>
                
                <Button Content="Filtrar" Command="{Binding LoadDataCommand}" MinWidth="80" Padding="15,0" Background="{StaticResource SuccessBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand" Margin="0,0,10,0"/>
                <Button Content="Excel" Command="{Binding ExportToCsvCommand}" MinWidth="60" Padding="15,0" Background="{StaticResource PrimaryBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand" Margin="0,0,10,0"/>
                <Button Content="PDF" Command="{Binding ExportEndOfDayPdfCommand}" MinWidth="60" Padding="15,0" Background="{StaticResource ErrorBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand"/>
            </StackPanel>"""

filters_new = """            <StackPanel Grid.Column="2" Orientation="Horizontal" VerticalAlignment="Center">
                <Button Content="Hoy" Command="{Binding FilterTodayCommand}" Padding="10,5" Background="{StaticResource BackgroundBrush}" Foreground="{StaticResource TextPrimaryBrush}" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Cursor="Hand" Margin="0,0,5,0">
                    <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="4"/></Style></Button.Resources>
                </Button>
                <Button Content="Última Sem." Command="{Binding FilterLastWeekCommand}" Padding="10,5" Background="{StaticResource BackgroundBrush}" Foreground="{StaticResource TextPrimaryBrush}" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Cursor="Hand" Margin="0,0,5,0">
                    <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="4"/></Style></Button.Resources>
                </Button>
                <Button Content="Este Mes" Command="{Binding FilterThisMonthCommand}" Padding="10,5" Background="{StaticResource BackgroundBrush}" Foreground="{StaticResource TextPrimaryBrush}" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" Cursor="Hand" Margin="0,0,15,0">
                    <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="4"/></Style></Button.Resources>
                </Button>
                
                <TextBlock Text="Desde:" VerticalAlignment="Center" Margin="0,0,5,0" Foreground="{StaticResource TextSecondaryBrush}"/>
                <DatePicker SelectedDate="{Binding StartDate}" Width="105" Margin="0,0,10,0" Background="{StaticResource SurfaceBrush}"/>
                
                <TextBlock Text="Hasta:" VerticalAlignment="Center" Margin="0,0,5,0" Foreground="{StaticResource TextSecondaryBrush}"/>
                <DatePicker SelectedDate="{Binding EndDate}" Width="105" Margin="0,0,10,0" Background="{StaticResource SurfaceBrush}"/>
                
                <Button Content="🔍" Command="{Binding LoadDataCommand}" ToolTip="Filtrar" MinWidth="40" Padding="10,0" Background="{StaticResource SuccessBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand" Margin="0,0,10,0">
                    <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="4"/></Style></Button.Resources>
                </Button>
                <Button Content="Excel" Command="{Binding ExportToCsvCommand}" MinWidth="60" Padding="15,0" Background="{StaticResource PrimaryBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand" Margin="0,0,10,0">
                    <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="4"/></Style></Button.Resources>
                </Button>
                <Button Content="PDF" Command="{Binding ExportEndOfDayPdfCommand}" MinWidth="60" Padding="15,0" Background="{StaticResource ErrorBrush}" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand">
                    <Button.Resources><Style TargetType="Border"><Setter Property="CornerRadius" Value="4"/></Style></Button.Resources>
                </Button>
            </StackPanel>"""

content = content.replace(filters_old, filters_new)

# 2. Update TabControl
tabcontrol_old = """        <TabControl Grid.Row="3" Background="Transparent" BorderThickness="0" Margin="-4,0,-4,0">"""
tabcontrol_new = """        <TabControl Grid.Row="3" Background="Transparent" BorderThickness="0" Margin="-4,0,-4,0" TabStripPlacement="Left">
            <TabControl.Resources>
                <Style TargetType="TabItem">
                    <Setter Property="Padding" Value="20,15"/>
                    <Setter Property="Margin" Value="0,0,0,5"/>
                    <Setter Property="Background" Value="Transparent"/>
                    <Setter Property="Foreground" Value="{StaticResource TextSecondaryBrush}"/>
                    <Setter Property="BorderThickness" Value="3,0,0,0"/>
                    <Setter Property="BorderBrush" Value="Transparent"/>
                    <Setter Property="Cursor" Value="Hand"/>
                    <Setter Property="Template">
                        <Setter.Value>
                            <ControlTemplate TargetType="TabItem">
                                <Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}">
                                    <ContentPresenter ContentSource="Header" Margin="{TemplateBinding Padding}" RecognizesAccessKey="True"/>
                                </Border>
                                <ControlTemplate.Triggers>
                                    <Trigger Property="IsSelected" Value="True">
                                        <Setter Property="Background" Value="#F1F5F9"/>
                                        <Setter Property="Foreground" Value="{StaticResource PrimaryBrush}"/>
                                        <Setter Property="BorderBrush" Value="{StaticResource PrimaryBrush}"/>
                                        <Setter Property="FontWeight" Value="SemiBold"/>
                                    </Trigger>
                                    <MultiTrigger>
                                        <MultiTrigger.Conditions>
                                            <Condition Property="IsSelected" Value="False"/>
                                            <Condition Property="IsMouseOver" Value="True"/>
                                        </MultiTrigger.Conditions>
                                        <Setter Property="Background" Value="#F8FAFC"/>
                                        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
                                    </MultiTrigger>
                                </ControlTemplate.Triggers>
                            </ControlTemplate>
                        </Setter.Value>
                    </Setter>
                </Style>
            </TabControl.Resources>"""

content = content.replace(tabcontrol_old, tabcontrol_new)

# 3. Update Chart
chart_old = """                    <ItemsControl Grid.Row="0" ItemsSource="{Binding DailySales}" Height="150" Margin="0,0,0,10">
                        <ItemsControl.ItemsPanel>
                            <ItemsPanelTemplate>
                                <UniformGrid Rows="1" />
                            </ItemsPanelTemplate>
                        </ItemsControl.ItemsPanel>
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Grid Margin="5,0" VerticalAlignment="Bottom" ToolTip="{Binding TotalRevenue, StringFormat=C}">
                                    <Grid.RowDefinitions>
                                        <RowDefinition Height="*"/>
                                        <RowDefinition Height="Auto"/>
                                    </Grid.RowDefinitions>
                                    <Border Background="{StaticResource SuccessBrush}" Width="30" VerticalAlignment="Bottom" Height="{Binding ChartHeight}" CornerRadius="4,4,0,0" />
                                    <TextBlock Grid.Row="1" Text="{Binding Date, StringFormat='dd/MM'}" HorizontalAlignment="Center" Margin="0,5,0,0" FontSize="11" Foreground="{StaticResource TextSecondaryBrush}"/>
                                </Grid>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>"""

chart_new = """                    <Border Grid.Row="0" Background="{StaticResource SurfaceBrush}" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" CornerRadius="8" Padding="20,20,20,10" Margin="0,0,0,15">
                        <Grid Height="180">
                            <!-- Reference lines -->
                            <UniformGrid Rows="4" Columns="1">
                                <Border BorderBrush="#F1F5F9" BorderThickness="0,1,0,0"/>
                                <Border BorderBrush="#F1F5F9" BorderThickness="0,1,0,0"/>
                                <Border BorderBrush="#F1F5F9" BorderThickness="0,1,0,0"/>
                                <Border BorderBrush="#F1F5F9" BorderThickness="0,1,0,1"/>
                            </UniformGrid>
                            
                            <!-- Bars -->
                            <ItemsControl ItemsSource="{Binding DailySales}">
                                <ItemsControl.ItemsPanel>
                                    <ItemsPanelTemplate>
                                        <UniformGrid Rows="1" />
                                    </ItemsPanelTemplate>
                                </ItemsControl.ItemsPanel>
                                <ItemsControl.ItemTemplate>
                                    <DataTemplate>
                                        <Grid Margin="5,0" VerticalAlignment="Bottom" ToolTip="{Binding TotalRevenue, StringFormat=C}">
                                            <Grid.RowDefinitions>
                                                <RowDefinition Height="*"/>
                                                <RowDefinition Height="Auto"/>
                                            </Grid.RowDefinitions>
                                            
                                            <!-- Gradient-like modern bar -->
                                            <Border Width="35" VerticalAlignment="Bottom" Height="{Binding ChartHeight}" CornerRadius="4,4,0,0">
                                                <Border.Background>
                                                    <LinearGradientBrush StartPoint="0,0" EndPoint="0,1">
                                                        <GradientStop Color="#34D399" Offset="0"/>
                                                        <GradientStop Color="#059669" Offset="1"/>
                                                    </LinearGradientBrush>
                                                </Border.Background>
                                            </Border>
                                            
                                            <TextBlock Grid.Row="1" Text="{Binding Date, StringFormat='dd/MM'}" HorizontalAlignment="Center" Margin="0,8,0,0" FontSize="12" FontWeight="SemiBold" Foreground="{StaticResource TextSecondaryBrush}"/>
                                        </Grid>
                                    </DataTemplate>
                                </ItemsControl.ItemTemplate>
                            </ItemsControl>
                        </Grid>
                    </Border>"""

content = content.replace(chart_old, chart_new)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
    
print("Updated ReportsWindow.xaml")
