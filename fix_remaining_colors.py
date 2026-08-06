import os
import re

replacements = {
    r'Background="#10B981"': 'Background="{StaticResource SuccessBrush}"',
    r'Foreground="#10B981"': 'Foreground="{StaticResource SuccessBrush}"',
    
    r'Background="#8B5CF6"': 'Background="{StaticResource AccentBrush}"',
    r'Foreground="#8B5CF6"': 'Foreground="{StaticResource AccentBrush}"',
    
    r'Background="#F59E0B"': 'Background="{StaticResource WarningBrush}"',
    r'Foreground="#F59E0B"': 'Foreground="{StaticResource WarningBrush}"',
    
    r'Background="#EF4444"': 'Background="{StaticResource ErrorBrush}"',
    r'Foreground="#EF4444"': 'Foreground="{StaticResource ErrorBrush}"',
    
    r'Background="#E5E7EB"': 'Background="{StaticResource BorderBrush}"',
    r'HorizontalGridLinesBrush="#E5E7EB"': 'HorizontalGridLinesBrush="{StaticResource BorderBrush}"',
    
    r'Foreground="#374151"': 'Foreground="{StaticResource TextPrimaryBrush}"',
    r'Foreground="#111827"': 'Foreground="{StaticResource TextPrimaryBrush}"',
    r'Background="#111827"': 'Background="{StaticResource TextPrimaryBrush}"',
    
    r'Background="#F9FAFB"': 'Background="{StaticResource BackgroundBrush}"',
    r'Foreground="#6B7280"': 'Foreground="{StaticResource TextSecondaryBrush}"',
    
    r'HorizontalGridLinesBrush="#F3F4F6"': 'HorizontalGridLinesBrush="{StaticResource BorderBrush}"',
    
    r'Foreground="#991B1B"': 'Foreground="{StaticResource ErrorBrush}"',
    r'Background="#FEE2E2"': 'Background="#FEE2E2"', # We'll leave specific light red tint alone or replace if needed
    r'Foreground="#065F46"': 'Foreground="{StaticResource SuccessBrush}"',
    r'Background="#D1FAE5"': 'Background="#D1FAE5"', # We'll leave light green tint alone
    
    r'Foreground="#9CA3AF"': 'Foreground="{StaticResource TextSecondaryBrush}"',
}

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    new_content = content
    for pattern, repl in replacements.items():
        new_content = re.sub(pattern, repl, new_content, flags=re.IGNORECASE)
    
    if content != new_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Updated {filepath}")

for root, _, files in os.walk('./PosCore'):
    for file in files:
        if file.endswith('.xaml'):
            process_file(os.path.join(root, file))
