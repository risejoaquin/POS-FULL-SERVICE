with open("PosCore/Services/OrderService.cs", "r") as f:
    text = f.read()

text = text.replace('                var stockResult = product.ReduceStock(item.Quantity);\n                if (!stockResult.IsSuccess) return stockResult;', '                if (product.StockQuantity < item.Quantity) return Result.Failure("No hay suficiente stock.");\n                product.StockQuantity -= (int)item.Quantity;')

with open("PosCore/Services/OrderService.cs", "w") as f:
    f.write(text)
