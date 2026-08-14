import re

with open("PosCore/Services/ApiService.cs", "r") as f:
    content = f.read()

# Add a closing brace after the catch block on the same line if it ends with }
content = re.sub(r'catch \(Exception ex\) \{ ([^}]+) \}', r'catch (Exception ex) { \1 }\n    }', content)

with open("PosCore/Services/ApiService.cs", "w") as f:
    f.write(content)
