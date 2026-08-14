with open("PosServer/Services/TenantService.cs", "r") as f:
    text = f.read()

text = text.replace("public string GetUsername() => _username;", "public string GetUsername() => _username;\n        public void SetUserId(string userId) => _userId = userId;\n        public string GetUserId() => _userId;")
text = text.replace("private string _username = string.Empty;", "private string _username = string.Empty;\n        private string _userId = string.Empty;")

with open("PosServer/Services/TenantService.cs", "w") as f:
    f.write(text)
