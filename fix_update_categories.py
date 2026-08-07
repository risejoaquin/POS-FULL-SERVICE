with open('./PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
for line in lines:
    if "UpdateCategories();" not in line:
        new_lines.append(line)

with open('./PosCore/ViewModels/MainViewModel.cs', 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
