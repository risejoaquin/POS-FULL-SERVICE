import sys

def check_braces(filename):
    with open(filename, 'r', encoding='utf-8') as f:
        content = f.read()
    
    count = 0
    line_num = 1
    for char in content:
        if char == '\n':
            line_num += 1
        elif char == '{':
            count += 1
        elif char == '}':
            count -= 1
            if count < 0:
                print(f"{filename}: Unmatched '}}' at line {line_num}")
                return
    if count > 0:
        print(f"{filename}: Missing {count} '}}' at the end of file")
    elif count == 0:
        print(f"{filename}: Braces are balanced")

check_braces('./PosCore/ViewModels/MainViewModel.cs')
check_braces('./PosCore/ViewModels/ReturnsViewModel.cs')
check_braces('./PosCore/ViewModels/ShiftViewModel.cs')
