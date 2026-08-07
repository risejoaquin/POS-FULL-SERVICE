import re

with open('./PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
skip = False
for i, line in enumerate(lines):
    if 156 <= i + 1 <= 167:
        continue # Skip SelectCategory and duplicate fields
    if 417 <= i + 1 <= 418:
        continue # Skip empty [RelayCommand] lines
    if i + 1 == 435:
        # Add [RelayCommand] to OpenInventory
        new_lines.append("    [RelayCommand]\n")
    
    new_lines.append(line)

with open('./PosCore/ViewModels/MainViewModel.cs', 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
