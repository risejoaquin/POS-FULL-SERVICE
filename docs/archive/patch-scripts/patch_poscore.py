import re

with open("PosCore/PosCore.csproj", "r") as f:
    content = f.read()

if "<ProjectReference Include=\"../PosDomain/PosDomain.csproj\" />" not in content:
    content = content.replace("</Project>", "  <ItemGroup>\n    <ProjectReference Include=\"../PosDomain/PosDomain.csproj\" />\n    <ProjectReference Include=\"../PosApplication/PosApplication.csproj\" />\n    <ProjectReference Include=\"../PosInfrastructure/PosInfrastructure.csproj\" />\n  </ItemGroup>\n</Project>")

with open("PosCore/PosCore.csproj", "w") as f:
    f.write(content)
