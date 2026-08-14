with open("PosCore/Data/PosDbContext.cs", "r") as f:
    text = f.read()

text = text.replace("public DbSet<CashMovement> CashMovements { get; set; }", "public DbSet<CashMovement> CashMovements { get; set; }\n    public DbSet<InventoryMovement> InventoryMovements { get; set; }")

with open("PosCore/Data/PosDbContext.cs", "w") as f:
    f.write(text)
