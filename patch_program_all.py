with open('PosServer/Program.cs', 'r', encoding='utf-8') as f:
    content = f.read()

target = """    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Users\\" ADD COLUMN IF NOT EXISTS \\"LastUpdated\\" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;"); } catch { }"""

replacements = """    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Users\\" ADD COLUMN IF NOT EXISTS \\"LastUpdated\\" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;"); } catch (Exception e) { Console.WriteLine("Migration error: " + e.Message); }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Products\\" ADD COLUMN IF NOT EXISTS \\"LastUpdated\\" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Orders\\" ADD COLUMN IF NOT EXISTS \\"LastUpdated\\" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"OrderItems\\" ADD COLUMN IF NOT EXISTS \\"LastUpdated\\" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"CashRegisterShifts\\" ADD COLUMN IF NOT EXISTS \\"LastUpdated\\" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"ProductModifiers\\" ADD COLUMN IF NOT EXISTS \\"LastUpdated\\" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;"); } catch { }"""

content = content.replace(target, replacements)

with open('PosServer/Program.cs', 'w', encoding='utf-8') as f:
    f.write(content)
