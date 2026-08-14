with open("PosCore/ViewModels/ReturnsViewModel.cs", "r") as f:
    content = f.read()

content = content.replace("PosCore.Models", "PosDomain.Entities")

with open("PosCore/ViewModels/ReturnsViewModel.cs", "w") as f:
    f.write(content)
