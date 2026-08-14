with open('PosServer/Middlewares/PostgresTenantMiddleware.cs', 'r') as f:
    c = f.read()

c = c.replace("""await dbContext.Database.ExecuteSqlRawAsync($"SELECT set_config('app.current_tenant', '{tenantId}', false);");""",
"""await dbContext.Database.ExecuteSqlAsync($"SELECT set_config('app.current_tenant', {tenantId}, false);");""")

with open('PosServer/Middlewares/PostgresTenantMiddleware.cs', 'w') as f:
    f.write(c)
