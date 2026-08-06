import os
import re

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    new_content = content
    # For standardizing corner radius to 8
    new_content = re.sub(r'Property="CornerRadius" Value="[0-9]+"', 'Property="CornerRadius" Value="8"', new_content, flags=re.IGNORECASE)
    
    if content != new_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Updated {filepath}")

for root, _, files in os.walk('./PosCore'):
    for file in files:
        if file.endswith('.xaml'):
            process_file(os.path.join(root, file))
