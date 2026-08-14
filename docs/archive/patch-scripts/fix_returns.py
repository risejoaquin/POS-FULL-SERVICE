with open("PosCore/ViewModels/ReturnsViewModel.cs", "r") as f:
    lines = f.readlines()

new_lines = []
skip = False
for i, line in enumerate(lines):
    if i >= 172 and i <= 176: # 173 to 177 is index 172 to 176
        continue
    new_lines.append(line)

with open("PosCore/ViewModels/ReturnsViewModel.cs", "w") as f:
    f.writelines(new_lines)

