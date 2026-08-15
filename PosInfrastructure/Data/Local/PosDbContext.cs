using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosDomain.Entities;
using PosDomain.Interfaces;
using PosApplication.Interfaces.Server;

namespace PosInfrastructure.Data.Local;

public class PosDbContext : DbContext
{
    private readonly ITenantContext _sessionManager;

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<CashRegisterShift> CashRegisterShifts { get; set; }
    public DbSet<CashMovement> CashMovements { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ProductModifier> ProductModifiers { get; set; }
    public DbSet<ModifierOption> ModifierOptions { get; set; }
    public DbSet<ProductModifierLink> ProductModifierLinks { get; set; }
    
    // Insumos y Recetas
    public DbSet<Supply> Supplies { get; set; }
    public DbSet<RecipeItem> RecipeItems { get; set; }

    public PosDbContext(DbContextOptions<PosDbContext> options, ITenantContext sessionManager) : base(options)
    {
        _sessionManager = sessionManager;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().ToTable(t => t.HasCheckConstraint("CK_Product_StockQuantity_NonNegative", "\"StockQuantity\" >= 0"));
        modelBuilder.Entity<Supply>().ToTable(t => t.HasCheckConstraint("CK_Supply_Stock_NonNegative", "\"Stock\" >= 0"));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.FindProperty("RowVersion") != null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<uint>("RowVersion")
                    .IsConcurrencyToken()
                    .ValueGeneratedNever()
                    .HasDefaultValue(1u);
            }
        }
        
        // Optimización SQLite: Índices
        modelBuilder.Entity<Product>()
            
            .HasIndex(p => p.Barcode)
            .IsUnique();
        
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderDate);
            
        modelBuilder.Entity<OutboxMessage>()
            .HasIndex(om => new { om.ProcessedAt, om.CreatedAt });
            
        modelBuilder.Entity<CashRegisterShift>()
            .HasIndex(crs => crs.TenantId);
            
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        var dictConverter = new ValueConverter<Dictionary<string, object>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new Dictionary<string, object>()
        );
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var dictComparer = new ValueComparer<Dictionary<string, object>>(
            (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
            c => c == null ? 0 : JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
            c => c == null ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions) ?? new Dictionary<string, object>()
        );

        modelBuilder.Entity<Product>()
            .Property(e => e.CustomAttributes).HasConversion(dictConverter, dictComparer);
        modelBuilder.Entity<Order>().Property(e => e.CustomAttributes).HasConversion(dictConverter, dictComparer);
        modelBuilder.Entity<OrderItem>().Property(e => e.CustomAttributes).HasConversion(dictConverter, dictComparer);

            
        // Multi-Tenant: Filtro Global
        modelBuilder.Entity<Product>()
            .HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<Order>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<OutboxMessage>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<CashRegisterShift>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<CashMovement>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<ProductModifier>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<ModifierOption>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<ProductModifierLink>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<Supply>()
            .HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
        modelBuilder.Entity<RecipeItem>().HasQueryFilter(e => e.TenantId == _sessionManager.GetTenantId());
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AssignTenantIdToAddedEntities();
        UpdateLastUpdatedField();
        GenerateOutboxMessages();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = "Data Source=pos_local.db;Default Timeout=30;";
            optionsBuilder.UseSqlite(connectionString);
        }
    }

    public void InitializeDatabaseSettings()
    {
        Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");
    }

    public override int SaveChanges()
    {
        AssignTenantIdToAddedEntities();
        UpdateLastUpdatedField();
        GenerateOutboxMessages();
        return base.SaveChanges();
    }

    private void GenerateOutboxMessages()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added || e.State == Microsoft.EntityFrameworkCore.EntityState.Modified || e.State == Microsoft.EntityFrameworkCore.EntityState.Deleted)
            .ToList();

        var jsonOptions = new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        foreach (var entry in entries)
        {
            if (entry.Entity is OutboxMessage) continue;
            if (entry.Entity is OrderItem || entry.Entity is CashMovement || entry.Entity is ProductModifierLink || entry.Entity is RecipeItem) continue;

            var entityType = entry.Entity.GetType().Name;
            string eventType = entry.State switch
            {
                Microsoft.EntityFrameworkCore.EntityState.Added => $"{entityType}Created",
                Microsoft.EntityFrameworkCore.EntityState.Modified => $"{entityType}Updated",
                Microsoft.EntityFrameworkCore.EntityState.Deleted => $"{entityType}Deleted",
                _ => "Unknown"
            };

            if (entry.Entity is Order ord && entry.State == Microsoft.EntityFrameworkCore.EntityState.Modified && ord.IsReturned)
            {
                eventType = "OrderReturned";
            }
            if (entry.Entity is CashRegisterShift sh)
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added) eventType = "ShiftOpened";
                else if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Modified && sh.ClosedAt != null) eventType = "ShiftClosed";
            }

            object entityToSerialize = entry.Entity;

            if (entry.Entity is Order order)
            {
                entityToSerialize = new Order
                {
                    Id = order.Id,
                    Status = order.Status,
                    OrderDate = order.OrderDate,
                    CustomerName = order.CustomerName,
                    SubTotal = order.SubTotal,
                    TaxAmount = order.TaxAmount,
                    TotalAmount = order.TotalAmount,
                    IsSynced = order.IsSynced,
                    LastUpdated = order.LastUpdated,
                    IsReturned = order.IsReturned,
                    ReturnReason = order.ReturnReason,
                    AuthorizedBy = order.AuthorizedBy,
                    CreatedById = order.CreatedById,

                    PaymentDetails = order.PaymentDetails,
                    TenantId = order.TenantId,
                    ClientSideId = order.ClientSideId,
                    CustomAttributes = order.CustomAttributes,
                    Payments = order.Payments?.Where(p => p != null).Select(p => new Payment
                    {
                        Amount = p.Amount,
                        Method = p.Method,
                        PaymentDate = p.PaymentDate,
                        ShiftId = p.ShiftId,
                        IdempotencyKey = p.IdempotencyKey,
                        TenantId = p.TenantId
                    }).ToList() ?? new List<Payment>(),
                    Items = order.Items?.Where(i => i != null).Select(i => new OrderItem

                    {
                        Id = i.Id,
                        OrderId = i.OrderId,
                        ProductId = i.ProductId,
                        ProductBarcode = i.ProductBarcode,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Discount = i.Discount,
                        Notes = i.Notes,
                        LastUpdated = i.LastUpdated,
                        TenantId = i.TenantId,
                        CustomAttributes = i.CustomAttributes
                    }).ToList() ?? new System.Collections.Generic.List<OrderItem>()
                };
            }
            else if (entry.Entity is CashRegisterShift shift)
            {
                entityToSerialize = new CashRegisterShift
                {
                    Id = shift.Id,
                    OpenedAt = shift.OpenedAt,
                    ClosedAt = shift.ClosedAt,
                    OpenedBy = shift.OpenedBy,
                    ClosedBy = shift.ClosedBy,
                    StartingCash = shift.StartingCash,
                    ExpectedEndingCash = shift.ExpectedEndingCash,
                    ActualEndingCash = shift.ActualEndingCash,
                    Difference = shift.Difference,
                    IsClosed = shift.IsClosed,
                    LastUpdated = shift.LastUpdated,
                    TenantId = shift.TenantId,
                    Movements = shift.Movements?.Where(m => m != null).Select(m => new CashMovement
                    {
                        Id = m.Id,
                        ShiftId = m.ShiftId,
                        Type = m.Type,
                        Amount = m.Amount,
                        Reason = m.Reason,
                        CreatedBy = m.CreatedBy,
                        CreatedAt = m.CreatedAt,
                        TenantId = m.TenantId
                    }).ToList() ?? new System.Collections.Generic.List<CashMovement>()
                };
            }
            else if (entry.Entity is Product product)
            {
                entityToSerialize = new Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Barcode = product.Barcode,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    MinStockThreshold = product.MinStockThreshold,
                    Category = product.Category,
                    ImagePath = product.ImagePath,
                    IsActive = product.IsActive,
                    LastUpdated = product.LastUpdated,
                    TenantId = product.TenantId,
                    CustomAttributes = product.CustomAttributes
                };
            }

            else if (entry.Entity is User user)
            {
                entityToSerialize = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Pin = null, // Never serialize PIN in outbox
                    PasswordHash = null, // Never serialize PasswordHash in outbox
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastUpdated = user.LastUpdated,
                    TenantId = user.TenantId
                };
            }
            var payload = JsonSerializer.Serialize(entityToSerialize, jsonOptions);

            OutboxMessages.Add(new OutboxMessage
            {
                EventType = eventType,
                Payload = payload,
                CreatedAt = DateTime.Now,
                TenantId = _sessionManager.GetTenantId() ?? throw new InvalidOperationException("No active session or TenantId found.")
            });
        }
    }

    private void AssignTenantIdToAddedEntities()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            var tenantProperty = entry.Entity.GetType().GetProperty("TenantId");
            if (tenantProperty != null)
            {
                var currentValue = tenantProperty.GetValue(entry.Entity) as string;
                if (string.IsNullOrEmpty(currentValue))
                {
                    tenantProperty.SetValue(entry.Entity, _sessionManager.GetTenantId());
                }
            }
        }
    }

    private void UpdateLastUpdatedField()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            var lastUpdatedProperty = entry.Entity.GetType().GetProperty("LastUpdated");
            if (lastUpdatedProperty != null)
            {
                lastUpdatedProperty.SetValue(entry.Entity, DateTime.UtcNow);
            }
        }
    }
}
