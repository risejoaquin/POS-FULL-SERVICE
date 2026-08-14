with open("PosCore/ViewModels/ReturnsViewModel.cs", "r") as f:
    content = f.read()

content = content.replace("PosCore.Models.OrderStatus.Refunded); // Si devolvieron todo parcialmente\n            }", "PosDomain.Entities.OrderStatus.Refunded); // Si devolvieron todo parcialmente\n            }")

with open("PosCore/ViewModels/ReturnsViewModel.cs", "w") as f:
    f.write(content)
