import re

with open('./PosServer/Program.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the specific ALTER TABLE statements with SQLite compatible ones
old_sql = '''    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Users\\" ADD COLUMN IF NOT EXISTS \\"PasswordHash\\" text DEFAULT '';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pgcrypto;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("UPDATE \\"Users\\" SET \\"PasswordHash\\" = crypt(\\"Pin\\", gen_salt('bf')) WHERE \\"Pin\\" IS NOT NULL AND \\"Pin\\" != '';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Users\\" DROP COLUMN IF EXISTS \\"Pin\\";"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Products\\" ADD COLUMN IF NOT EXISTS \\"CustomAttributes\\" jsonb DEFAULT '{}';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Products\\" ALTER COLUMN \\"CustomAttributes\\" TYPE jsonb USING \\"CustomAttributes\\"::text::jsonb;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Orders\\" ADD COLUMN IF NOT EXISTS \\"CustomAttributes\\" jsonb DEFAULT '{}';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Orders\\" ALTER COLUMN \\"CustomAttributes\\" TYPE jsonb USING \\"CustomAttributes\\"::text::jsonb;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"OrderItems\\" ADD COLUMN IF NOT EXISTS \\"CustomAttributes\\" jsonb DEFAULT '{}';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"OrderItems\\" ALTER COLUMN \\"CustomAttributes\\" TYPE jsonb USING \\"CustomAttributes\\"::text::jsonb;"); } catch { }'''

new_sql = '''    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Users\\" ADD COLUMN \\"PasswordHash\\" text DEFAULT '';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pgcrypto;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("UPDATE \\"Users\\" SET \\"PasswordHash\\" = crypt(\\"Pin\\", gen_salt('bf')) WHERE \\"Pin\\" IS NOT NULL AND \\"Pin\\" != '';"); } catch { }
    // try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Users\\" DROP COLUMN \\"Pin\\";"); } catch { } // SQLite has issues dropping columns
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Products\\" ADD COLUMN \\"CustomAttributes\\" text DEFAULT '{}';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Orders\\" ADD COLUMN \\"CustomAttributes\\" text DEFAULT '{}';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"OrderItems\\" ADD COLUMN \\"CustomAttributes\\" text DEFAULT '{}';"); } catch { }'''

content = content.replace(old_sql, new_sql)

with open('./PosServer/Program.cs', 'w', encoding='utf-8') as f:
    f.write(content)
