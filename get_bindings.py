import os
import re

def parse_xaml_bindings(file):
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()
    bindings = set(re.findall(r'Binding\s+([A-Za-z0-9_]+)', content))
    return bindings

def parse_cs_members(file):
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()
    # Basic heuristic for members
    members = set(re.findall(r'(?:public|private|protected)\s+(?:async\s+)?(?:partial\s+)?[A-Za-z0-9_<>]+\s+([A-Za-z0-9_]+)', content))
    return members

# We'll just look at XAML files and see what they bind to, and check if it exists in their respective ViewModel
