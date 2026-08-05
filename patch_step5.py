import re

with open('PosBuilder/Views/Step5Users.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

# Add xmlns if not present
if 'xmlns:helpers="clr-namespace:PosBuilder.Helpers"' not in content:
    content = content.replace('xmlns:models="clr-namespace:PosBuilder.Models"', 
                              'xmlns:models="clr-namespace:PosBuilder.Models"\n             xmlns:helpers="clr-namespace:PosBuilder.Helpers"')

# Replace TextBox with PasswordBox for Extra Users
target = """<!-- Using TextBox for Extra User passwords to simplify bindings, since PasswordBox doesn't bind easily in WPF -->
                                        <TextBox Text="{Binding Password, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                            <TextBox.Resources>
                                                <Style TargetType="Border">
                                                    <Setter Property="CornerRadius" Value="4"/>
                                                </Style>
                                            </TextBox.Resources>
                                        </TextBox>"""
replacement = """<PasswordBox helpers:PasswordBoxHelper.BindPassword="true" helpers:PasswordBoxHelper.BoundPassword="{Binding Password, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                            <PasswordBox.Resources>
                                                <Style TargetType="Border">
                                                    <Setter Property="CornerRadius" Value="4"/>
                                                </Style>
                                            </PasswordBox.Resources>
                                        </PasswordBox>"""
content = content.replace(target, replacement)

with open('PosBuilder/Views/Step5Users.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
