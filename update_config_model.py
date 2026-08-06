with open('PosBuilder/Models/ConfigModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

if 'public List<UserModel> ExtraUsers' not in content:
    content = content.replace('namespace PosBuilder.Models\n{', 'using System.Collections.Generic;\n\nnamespace PosBuilder.Models\n{')
    content = content.replace('        public string Environment { get; set; } = "";', '        public string Environment { get; set; } = "";\n        public List<UserModel> ExtraUsers { get; set; } = new List<UserModel>();')

with open('PosBuilder/Models/ConfigModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
print("ConfigModel updated")
