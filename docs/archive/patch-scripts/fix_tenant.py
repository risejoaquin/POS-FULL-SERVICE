with open("PosDomain/Interfaces/ITenantService.cs", "r") as f:
    text = f.read()

text = text.replace("string GetUsername();", "string GetUsername();\n    string GetUserId();\n    void SetUserId(string userId);")

with open("PosDomain/Interfaces/ITenantService.cs", "w") as f:
    f.write(text)
