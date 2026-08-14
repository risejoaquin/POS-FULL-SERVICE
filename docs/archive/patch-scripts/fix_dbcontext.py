with open("PosCore/Data/PosDbContext.cs", "r") as f:
    text = f.read()

text = text.replace("public DbSet<CashMovement> CashMovements { get; set; } = null!;", "public DbSet<CashMovement> CashMovements { get; set; } = null!;\n    public DbSet<InventoryMovement> InventoryMovements { get; set; } = null!;")

with open("PosCore/Data/PosDbContext.cs", "w") as f:
    f.write(text)
