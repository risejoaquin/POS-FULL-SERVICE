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
                
                obs_props = set()
                relay_cmds = set()
                
                for i, line in enumerate(lines):
                    line_strip = line.strip()
                    
                    if line_strip.startswith("private ") or line_strip.startswith("public "):
                        # Find method or property name
                        match = re.search(r'(?:private|public)(?:(?:\s+async)?\s+[\w<>,]+)+\s+(\w+)\s*(?:\(|;|=)', line_strip)
                        if match:
                            name = match.group(1)
                            
                            # Check attributes
                            has_obs = False
                            has_relay = False
                            for j in range(i-1, max(-1, i-3), -1):
                                if "[ObservableProperty]" in lines[j]: has_obs = True
                                if "[RelayCommand]" in lines[j]: has_relay = True
                            
                            if has_obs:
                                if name in obs_props:
                                    errors.append(f"{path}:{i+1} Duplicate [ObservableProperty] for {name}")
                                obs_props.add(name)
                            if has_relay:
                                if name in relay_cmds:
                                    errors.append(f"{path}:{i+1} Duplicate [RelayCommand] for {name}")
                                relay_cmds.add(name)
                    
                    if line_strip == "[RelayCommand]" and i < len(lines)-1 and lines[i+1].strip() == "[RelayCommand]":
                        errors.append(f"{path}:{i+1} Consecutive [RelayCommand]")

    for e in errors:
        print(e)
    if not errors:
        print("No duplicates found in C# files.")

audit_cs()
