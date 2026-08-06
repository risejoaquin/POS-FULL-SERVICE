import re

with open('./PosBuilder/Views/Step5Users.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix mismatched tags
content = content.replace(
    '''<TextBox Text="{Binding AdminUser, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                <PasswordBox.Resources>''',
    '''<TextBox Text="{Binding AdminUser, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                <TextBox.Resources>'''
)

content = content.replace(
    '''<TextBox Text="{Binding EmployeeUser, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                <PasswordBox.Resources>''',
    '''<TextBox Text="{Binding EmployeeUser, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                <TextBox.Resources>'''
)

content = content.replace(
    '''<TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                            <PasswordBox.Resources>''',
    '''<TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                            <TextBox.Resources>'''
)

content = content.replace(
    '''</Style>
                                </PasswordBox.Resources>
                            </TextBox>''',
    '''</Style>
                                </TextBox.Resources>
                            </TextBox>'''
)

content = content.replace(
    '''</Style>
                                            </PasswordBox.Resources>
                                        </TextBox>''',
    '''</Style>
                                            </TextBox.Resources>
                                        </TextBox>'''
)

content = content.replace(
    '''<PasswordBox local:PasswordBoxHelper.BindPassword="True" local:PasswordBoxHelper.BoundPassword="{Binding Password, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                            <PasswordBox.Resources>
                                                <Style TargetType="Border">
                                                    <Setter Property="CornerRadius" Value="8"/>
                                                </Style>
                                            </PasswordBox.Resources>
                                        </TextBox>''',
    '''<PasswordBox local:PasswordBoxHelper.BindPassword="True" local:PasswordBoxHelper.BoundPassword="{Binding Password, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                            <PasswordBox.Resources>
                                                <Style TargetType="Border">
                                                    <Setter Property="CornerRadius" Value="8"/>
                                                </Style>
                                            </PasswordBox.Resources>
                                        </PasswordBox>'''
)

with open('./PosBuilder/Views/Step5Users.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
