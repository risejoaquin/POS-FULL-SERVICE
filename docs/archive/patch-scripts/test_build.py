import subprocess
import re

print("Running dotnet build Pos.sln")
result = subprocess.run(["/root/.dotnet/dotnet", "build", "Pos.sln"], capture_output=True, text=True)

errors = []
for line in result.stdout.split('\n'):
    if "error CS" in line:
        errors.append(line)

print(f"Found {len(errors)} errors")
if len(errors) > 0:
    for e in errors[:5]:
        print(e)
