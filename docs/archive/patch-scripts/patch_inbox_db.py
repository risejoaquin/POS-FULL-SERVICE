with open('PosInfrastructure/Data/Server/CentralDbContext.cs', 'r') as f:
    c = f.read()

c = c.replace("""        modelBuilder.Entity<OutboxMessage>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });""", """        modelBuilder.Entity<OutboxMessage>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });
        modelBuilder.Entity<InboxMessage>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });""")

with open('PosInfrastructure/Data/Server/CentralDbContext.cs', 'w') as f:
    f.write(c)
