import re

with open('./PosCore/Views/Controls/CartPanelControl.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

old_impuestos = """<TextBlock Text="0.00" FontSize="16" Foreground="{StaticResource TextSecondaryBrush}"/>"""
new_impuestos = """<TextBlock Text="{Binding Taxes, StringFormat=C}" FontSize="16" Foreground="{StaticResource TextSecondaryBrush}"/>"""
content = content.replace(old_impuestos, new_impuestos)

old_subtotal = """<TextBlock Text="{Binding Total, StringFormat=C}" FontSize="16" Foreground="{StaticResource TextSecondaryBrush}"/>"""
new_subtotal = """<TextBlock Text="{Binding SubTotal, StringFormat=C}" FontSize="16" Foreground="{StaticResource TextSecondaryBrush}"/>"""

# Just replacing the first occurrence which is usually Subtotal. Wait, let's be more precise.
old_subtotal_block = """                    <TextBlock Text="Subtotal:" FontSize="16" Foreground="{StaticResource TextSecondaryBrush}"/>
                    <TextBlock Text="{Binding Total, StringFormat=C}" FontSize="16" Foreground="{StaticResource TextSecondaryBrush}"/>"""
new_subtotal_block = """                    <TextBlock Text="Subtotal:" FontSize="16" Foreground="{StaticResource TextSecondaryBrush}"/>
                    <TextBlock Text="{Binding SubTotal, StringFormat=C}" FontSize="16" Foreground="{StaticResource TextSecondaryBrush}"/>"""
content = content.replace(old_subtotal_block, new_subtotal_block)

# Remove the comment
content = re.sub(r'<!-- Impuestos is not in VM by default.*?-->', '', content, flags=re.DOTALL)

with open('./PosCore/Views/Controls/CartPanelControl.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
