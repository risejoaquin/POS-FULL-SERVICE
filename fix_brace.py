filepath = './PosCore/ViewModels/MainViewModel.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# Line 513 is index 512
if lines[512].strip() == '}':
    del lines[512]
    with open(filepath, 'w', encoding='utf-8') as f:
        f.writelines(lines)
    print("Removed line 513")
else:
    print(f"Line 513 is not a closing brace: {lines[512]}")
