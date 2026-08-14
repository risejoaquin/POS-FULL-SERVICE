import re

with open("PosServer/PosServer.csproj", "r") as f:
    content = f.read()

content = re.sub(r'<ProjectReference Include="\.\./PosCore/PosCore\.csproj" />', '<ProjectReference Include="../PosApplication/PosApplication.csproj" />\n    <ProjectReference Include="../PosInfrastructure/PosInfrastructure.csproj" />', content)

with open("PosServer/PosServer.csproj", "w") as f:
    f.write(content)
