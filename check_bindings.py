import os
import re

def get_commands_from_xaml():
    commands = set()
    for root, _, files in os.walk('./PosCore'):
        for file in files:
            if file.endswith('.xaml'):
                with open(os.path.join(root, file), 'r', encoding='utf-8') as f:
                    content = f.read()
                # Find Command="{Binding XXX}"
                matches = re.findall(r'Command="\{Binding\s+([A-Za-z0-9_]+)', content)
                for m in matches:
                    commands.add(m)
    return commands

def get_methods_and_commands_from_cs():
    defined = set()
    for root, _, files in os.walk('./PosCore'):
        for file in files:
            if file.endswith('.cs'):
                with open(os.path.join(root, file), 'r', encoding='utf-8') as f:
                    lines = f.readlines()
                for i, line in enumerate(lines):
                    if "[RelayCommand]" in line:
                        # get next line method name
                        for j in range(i+1, min(i+5, len(lines))):
                            m = re.search(r'(?:private|public)\s+(?:async\s+)?(?:Task|void)\s+([A-Za-z0-9_]+)', lines[j])
                            if m:
                                name = m.group(1)
                                if name.endswith("Async"):
                                    name = name[:-5]
                                defined.add(name + "Command")
                                break
                    # Manual ICommand or RelayCommand properties
                    m = re.search(r'public\s+(?:ICommand|IRelayCommand)\s+([A-Za-z0-9_]+)', line)
                    if m:
                        defined.add(m.group(1))
    return defined

xaml_cmds = get_commands_from_xaml()
cs_cmds = get_methods_and_commands_from_cs()

missing = xaml_cmds - cs_cmds
print("Commands used in XAML but not defined in CS (might be from other contexts or false positives):")
for c in missing:
    print(c)
