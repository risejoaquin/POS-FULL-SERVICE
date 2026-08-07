import os
import re

def audit_cs():
    errors = []
    for root, _, files in os.walk('./PosCore'):
        for file in files:
            if file.endswith('.cs'):
                path = os.path.join(root, file)
                with open(path, 'r', encoding='utf-8') as f:
                    lines = f.readlines()
                
                # Check for consecutive identical attributes or properties
                obs_props = set()
                relay_cmds = set()
                
                for i, line in enumerate(lines):
                    line_strip = line.strip()
                    
                    if line_strip.startswith("private") and " " in line_strip:
                        # Extract variable or method name
                        parts = line_strip.split()
                        if len(parts) >= 3:
                            name = parts[2].split('(')[0].rstrip(';')
                            # Look back for attributes
                            if i > 0 and "[ObservableProperty]" in lines[i-1]:
                                if name in obs_props:
                                    errors.append(f"{path}:{i+1} Duplicate [ObservableProperty] for {name}")
                                obs_props.add(name)
                            if i > 0 and "[RelayCommand]" in lines[i-1]:
                                if name in relay_cmds:
                                    errors.append(f"{path}:{i+1} Duplicate [RelayCommand] for {name}")
                                relay_cmds.add(name)
                    
                    if line_strip == "[RelayCommand]" and i < len(lines)-1 and "[RelayCommand]" in lines[i+1]:
                        errors.append(f"{path}:{i+1} Consecutive [RelayCommand]")

    for e in errors:
        print(e)
    if not errors:
        print("No duplicates found in C# files.")

audit_cs()
