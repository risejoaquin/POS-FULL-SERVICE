with open("PosApplication/UseCases/Orders/CreateOrderUseCase.cs", "r") as f:
    content = f.read()

content = content.replace("namespace PosCore.Services", "namespace PosApplication.UseCases.Orders")
content = content.replace("class OrderDomainService", "class CreateOrderUseCase")
content = content.replace("public OrderDomainService(", "public CreateOrderUseCase(")
content = content.replace("using PosCore.Models;", "using PosDomain.Models;\nusing PosDomain.Entities;")
content = content.replace("using PosCore.Interfaces;", "using PosDomain.Interfaces;")

with open("PosApplication/UseCases/Orders/CreateOrderUseCase.cs", "w") as f:
    f.write(content)
