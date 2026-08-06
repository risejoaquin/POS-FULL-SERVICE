with open('./PosBuilder/Views/Step5Users.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
'''                                            <PasswordBox.Resources>
                                                <Style TargetType="Border">
                                                    <Setter Property="CornerRadius" Value="8"/>
                                                </Style>
                                            </TextBox.Resources>
                                        </TextBox>''',
'''                                            <PasswordBox.Resources>
                                                <Style TargetType="Border">
                                                    <Setter Property="CornerRadius" Value="8"/>
                                                </Style>
                                            </PasswordBox.Resources>
                                        </PasswordBox>'''
)

with open('./PosBuilder/Views/Step5Users.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
