import os
import re

for i in range(1, 8):
    path = "PosBuilder/Views/"
    files = os.listdir(path)
    file_name = next((f for f in files if f.startswith(f"Step{i}") and f.endswith(".xaml")), None)
    if not file_name: continue
    filepath = os.path.join(path, file_name)
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Remove <TextBlock Text="Paso X:... />
    content = re.sub(r'<TextBlock\s+Text="Paso\s+\d+[^"]*"\s+FontSize="24"[^>]*/>\s*', '', content)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

print("Done")
