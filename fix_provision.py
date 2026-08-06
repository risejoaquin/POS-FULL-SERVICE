with open('./PosServer/Models/ProvisionRequest.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("PosBuilder.Models.UserModel", "ExtraUserDto")
content = content.replace("public class ProvisionRequest", "public class ExtraUserDto\n    {\n        public string Username { get; set; }\n        public string Password { get; set; }\n        public string Role { get; set; }\n    }\n\n    public class ProvisionRequest")

with open('./PosServer/Models/ProvisionRequest.cs', 'w', encoding='utf-8') as f:
    f.write(content)
