import os
import re

replacements = {
    r'Background="#F3F4F6"': 'Background="{StaticResource BackgroundBrush}"',
    r'Background="#F9FAFB"': 'Background="{StaticResource BackgroundBrush}"',
    r'Background="White"': 'Background="{StaticResource SurfaceBrush}"',
    r'Background="#FFFFFF"': 'Background="{StaticResource SurfaceBrush}"',
    
    r'BorderBrush="#E5E7EB"': 'BorderBrush="{StaticResource BorderBrush}"',
    r'BorderBrush="#D1D5DB"': 'BorderBrush="{StaticResource BorderBrush}"',
    
    r'Foreground="#1F2937"': 'Foreground="{StaticResource TextPrimaryBrush}"',
    r'Foreground="#111827"': 'Foreground="{StaticResource TextPrimaryBrush}"',
    
    r'Foreground="#6B7280"': 'Foreground="{StaticResource TextSecondaryBrush}"',
    r'Foreground="#4B5563"': 'Foreground="{StaticResource TextSecondaryBrush}"',
    
    r'Foreground="#EF4444"': 'Foreground="{StaticResource ErrorBrush}"',
    
    r'Background="#3B82F6"': 'Background="{StaticResource PrimaryBrush}"',
    r'Foreground="#3B82F6"': 'Foreground="{StaticResource PrimaryBrush}"',
    r'Background="#2D5F2E"': 'Background="{StaticResource PrimaryBrush}"',
    
    # Shadows
    r'<DropShadowEffect[^>]*>': '<DropShadowEffect Color="#020617" BlurRadius="16" ShadowDepth="4" Opacity="0.06" Direction="270" />'
}

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    new_content = content
    for pattern, repl in replacements.items():
        new_content = re.sub(pattern, repl, new_content, flags=re.IGNORECASE)
        
    # Also standardize some CornerRadius
    new_content = re.sub(r'CornerRadius="[0-9]+"', 'CornerRadius="8"', new_content)
    # Fix Padding on buttons if it's too small
    # Maybe standard button height etc. We'll rely on ButtonStyles for most if they use it.
    
    if content != new_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Updated {filepath}")

for root, _, files in os.walk('./PosCore'):
    for file in files:
        if file.endswith('.xaml'):
            process_file(os.path.join(root, file))

