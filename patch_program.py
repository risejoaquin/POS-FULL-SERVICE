import re

with open('PosServer/Program.cs', 'r', encoding='utf-8') as f:
    content = f.read()

target = """    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Users\\" ADD COLUMN IF NOT EXISTS \\"PasswordHash\\" text DEFAULT '';"); } catch { }"""
replacement = """    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Users\\" ADD COLUMN IF NOT EXISTS \\"PasswordHash\\" text DEFAULT '';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Users\\" ADD COLUMN IF NOT EXISTS \\"LastUpdated\\" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;"); } catch { }"""

content = content.replace(target, replacement)

with open('PosServer/Program.cs', 'w', encoding='utf-8') as f:
    f.write(content)
