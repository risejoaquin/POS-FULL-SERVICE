import re

with open('./PosBuilder/Views/Step5Users.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

# For the DataTemplate PasswordBox
old_pb = """                                    <StackPanel Grid.Column="2">
                                        <PasswordBox Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1">
                                            <PasswordBox.Resources>"""

new_pb = """                                    <StackPanel Grid.Column="2">
                                        <PasswordBox Padding="10,8" FontSize="13" BorderBrush="#CBD5E1" BorderThickness="1" PasswordChanged="ExtraUserPass_PasswordChanged">
                                            <PasswordBox.Resources>"""

content = content.replace(old_pb, new_pb)
# If the author used TextBox previously (because they said "no se pone oculta la contraseña cuando se escribe"), let's check what was there before I replace it.
