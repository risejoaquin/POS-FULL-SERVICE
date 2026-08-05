with open('PosBuilder/PosBuilder.csproj', 'r', encoding='utf-8') as f:
    content = f.read()

if '<Using Remove="System.Windows.Forms" />' not in content:
    content = content.replace('</PropertyGroup>', '</PropertyGroup>\n  <ItemGroup>\n    <Using Remove="System.Windows.Forms" />\n    <Using Remove="System.Drawing" />\n  </ItemGroup>')

with open('PosBuilder/PosBuilder.csproj', 'w', encoding='utf-8') as f:
    f.write(content)
print("csproj patched")
