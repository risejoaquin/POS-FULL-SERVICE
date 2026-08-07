import os
def check_braces(filename):
    with open(filename, 'r', encoding='utf-8') as f:
        content = f.read()
    
    count = 0
    for char in content:
        if char == '{': count += 1
        elif char == '}': count -= 1
        if count < 0:
            return False, "Negative count"
    if count > 0:
        return False, "Positive count"
    return True, "OK"

for root, _, files in os.walk('.'):
    for f in files:
        if f.endswith('.cs'):
            path = os.path.join(root, f)
            ok, msg = check_braces(path)
            if not ok:
                print(f"{path}: {msg}")
