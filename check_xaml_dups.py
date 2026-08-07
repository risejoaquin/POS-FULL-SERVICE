import os

def check_duplicate_attributes(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Very rudimentary check using regex
    import re
    # Match tags
    tags = re.findall(r'<([a-zA-Z0-9_:]+)([^>]*?)>', content)
    errors = []
    for tag, attrs_str in tags:
        # Extract attribute names
        attr_names = re.findall(r'([a-zA-Z0-9_:]+)=["\']', attrs_str)
        seen = set()
        for a in attr_names:
            if a in seen:
                errors.append(f"{filepath}: Duplicate attribute '{a}' in tag '{tag}'")
            seen.add(a)
    return errors

for root, _, files in os.walk('./PosCore'):
    for file in files:
        if file.endswith('.xaml'):
            errs = check_duplicate_attributes(os.path.join(root, file))
            for e in errs:
                print(e)
