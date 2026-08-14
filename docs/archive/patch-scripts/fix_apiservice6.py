with open("PosCore/Services/ApiService.cs", "r") as f:
    text = f.read()

text = text.replace("using PosCore.Models;", "")
text = text.replace("public async Task<LoginResponse?>", "public async Task<PosCore.Models.LoginResponse?>")
text = text.replace("ReadFromJsonAsync<LoginResponse>", "ReadFromJsonAsync<PosCore.Models.LoginResponse>")
text = text.replace("new LoginRequest", "new PosCore.Models.LoginRequest")

with open("PosCore/Services/ApiService.cs", "w") as f:
    f.write(text)

with open("PosCore/Services/IApiService.cs", "r") as f:
    text2 = f.read()
text2 = text2.replace("using PosCore.Models;", "")
text2 = text2.replace("Task<LoginResponse?>", "Task<PosCore.Models.LoginResponse?>")
with open("PosCore/Services/IApiService.cs", "w") as f:
    f.write(text2)

