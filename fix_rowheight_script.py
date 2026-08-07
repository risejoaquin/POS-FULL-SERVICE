import os
import re

def fix_rowheights(content):
    # Change RowHeight="40" to MinRowHeight="40" for fluidity
    return re.sub(r'RowHeight="(\d+)"', r'MinRowHeight="\1"', content)

def fix_horizontal_stackpanels_in_datagrids(content):
    # This might be tricky via regex. Instead, I'll replace specific known patterns.
    # Replace <StackPanel Orientation="Horizontal"> with <WrapPanel> for actions
    content = content.replace('<StackPanel Orientation="Horizontal">', '<WrapPanel>')
    content = content.replace('</StackPanel>', '</WrapPanel>')
    # But wait, this might break vertical StackPanels if we just blindly replace </StackPanel> 
    # Let's be more specific.
    return content

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    new_content = fix_rowheights(content)

    if new_content != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Fixed RowHeight in {filepath}")

for root, _, files in os.walk('.'):
    for f in files:
        if f.endswith('.xaml'):
            process_file(os.path.join(root, f))
