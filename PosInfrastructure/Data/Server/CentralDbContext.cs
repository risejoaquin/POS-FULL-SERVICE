using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using PosDomain.Entities;
using PosDomain.Interfaces;
using PosApplication.Interfaces.Server;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PosInfrastructure.Data.Server;

// PHASE 7C targeted CentralDbContext nullability remediation: non-null DbSet initialization and safe string conversion for audit/outbox metadata.

public class CentralDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public string CurrentTenantId 
    { 
        get 
        {
            return _tenantContext?.GetTenantId() ?? string.Empty;
        }
    }

    public CentralDbContext(DbContextOptions<CentralDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    // Constructor sin ITenantContext para herramientas de diseño
    public CentralDbContext(DbContextOptions<CentralDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<License> Licenses { get; set; } = null!;
    public DbSet<ProductModifier> ProductModifiers { get; set; } = null!;
    public DbSet<ModifierOption> ModifierOptions { get; set; } = null!;
    public DbSet<ProductModifierLink> ProductModifierLinks { get; set; } = null!;
    
    public DbSet<CashRegisterShift> CashRegisterShifts { get; set; } = null!;
    public DbSet<CashMovement> CashMovements { get; set; } = null!;
    public DbSet<Supply> Supplies { get; set; } = null!;
    public DbSet<InventoryMovement> InventoryMovements { get; set; } = null!;
    public DbSet<RecipeItem> RecipeItems { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().ToTable(t => t.HasCheckConstraint("CK_Product_StockQuantity_NonNegative", "\"StockQuantity\" >= 0"));
        modelBuilder.Entity<Supply>().ToTable(t => t.HasCheckConstraint("CK_Supply_Stock_NonNegative", "\"Stock\" >= 0"));
        
        // Conversor para Dictionary<string, object> a JSON string (que en Postgres se mapeará a jsonb)
        var dictConverter = new ValueConverter<Dictionary<string, object>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new Dictionary<string, object>()
        );
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        // Optimización de rendimiento: precalcular hashes de diccionarios serializados usando SHA256 para evitar evaluación O(n^2)
        var dictComparer = new ValueComparer<Dictionary<string, object>>(
            (c1, c2) => DictionaryHashCache.GetHash(c1) == DictionaryHashCache.GetHash(c2),
            c => DictionaryHashCache.GetHash(c).GetHashCode(),
            c => c == null ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!
        );

        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Username, u.TenantId })
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.TenantId);
        
        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.TenantId, p.Barcode })
            .IsUnique();
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.TenantId);
        
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.TenantId, o.OrderDate })
            .IsDescending(false, true);
            
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.TenantId, o.IdempotencyKey })
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" != ''");

        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.TenantId, p.IdempotencyKey })
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" != ''");
        
        modelBuilder.Entity<OrderItem>()
            .HasIndex(oi => oi.OrderId);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuración de CustomAttributes y Filtros Globales (Global Query Filters)
        
        modelBuilder.Entity<Product>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
            entity.Property(e => e.CustomAttributes).HasColumnType("jsonb").HasConversion(dictConverter, dictComparer);
        });

        modelBuilder.Entity<Order>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
            entity.Property(e => e.CustomAttributes).HasColumnType("jsonb").HasConversion(dictConverter, dictComparer);
        });

        modelBuilder.Entity<OrderItem>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
            entity.Property(e => e.CustomAttributes).HasColumnType("jsonb").HasConversion(dictConverter, dictComparer);
        });

        modelBuilder.Entity<User>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<CashRegisterShift>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
            entity.HasMany(s => s.Movements).WithOne(m => m.Shift).HasForeignKey(m => m.ShiftId);
        });

        modelBuilder.Entity<CashMovement>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });
        
        modelBuilder.Entity<Supply>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });
        
        modelBuilder.Entity<RecipeItem>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<License>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<ProductModifier>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });
        
        modelBuilder.Entity<ModifierOption>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<ProductModifierLink>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });


        modelBuilder.Entity<Payment>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<InventoryMovement>(entity => { entity.HasQueryFilter(e => e.TenantId == CurrentTenantId); });

        modelBuilder.Entity<OutboxMessage>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });
        modelBuilder.Entity<InboxMessage>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<AuditLog>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        // Configurar RowVersion (xmin en postgresql) para concurrencia optimista

    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        string currentTenant = CurrentTenantId;
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            // Enforcement de Multi-Tenant
            var tenantProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "TenantId");
            if (tenantProp != null && !string.IsNullOrEmpty(currentTenant))
            {
                if (entry.State == EntityState.Added && (tenantProp.CurrentValue == null || string.IsNullOrEmpty(tenantProp.CurrentValue.ToString())))
                {
                    tenantProp.CurrentValue = currentTenant;
                }
                else if (entry.State == EntityState.Modified && tenantProp.CurrentValue?.ToString() != currentTenant)
                {
                    // Evitar modificar datos de otro tenant
                    throw new UnauthorizedAccessException($"Multi-tenant violation: Attemped to modify entity of tenant {tenantProp.CurrentValue} using context of {currentTenant}");
                }
            }
        
            if (entry.Entity is OutboxMessage || entry.Entity is AuditLog) continue;

            // Update LastUpdated implicitly
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                var lastUpdatedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "LastUpdated");
                if (lastUpdatedProp != null)
                {
                    lastUpdatedProp.CurrentValue = DateTime.UtcNow;
                }
            }
            
            var username = _tenantContext?.GetUsername() ?? "System";

            var auditLog = new AuditLog
            {
                Action = entry.State.ToString(),
                EntityType = entry.Entity.GetType().Name,
                UserId = username,
                Timestamp = DateTime.UtcNow,
                TenantId = currentTenant
            };

            var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            if (idProp != null && idProp.CurrentValue != null)
            {
                auditLog.EntityId = idProp.CurrentValue.ToString() ?? string.Empty;
            }

            if (entry.State == EntityState.Modified)
            {
                var originalValues = new Dictionary<string, object?>();
                var currentValues = new Dictionary<string, object?>();
                
                foreach (var prop in entry.OriginalValues.Properties)
                {
                    if (prop.Name == "PasswordHash" || prop.Name == "Pin" || prop.Name == "RefreshToken" || prop.Name == "RefreshTokenExpiryTime" || prop.Name == "ProvisionKey" || prop.Name == "SecretKey")
                    {
                        continue;
                    }
                    var originalValue = entry.OriginalValues[prop];
                    var currentValue = entry.CurrentValues[prop];
                    if (!Equals(originalValue, currentValue))
                    {
                        originalValues[prop.Name] = originalValue;
                        currentValues[prop.Name] = currentValue;
                    }
                }
                
                auditLog.OldValues = JsonSerializer.Serialize(originalValues);
                auditLog.NewValues = JsonSerializer.Serialize(currentValues);
            }
            else if (entry.State == EntityState.Added)
            {
                var currentValues = new Dictionary<string, object?>();
                foreach (var prop in entry.CurrentValues.Properties)
                {
                    if (prop.Name == "PasswordHash" || prop.Name == "Pin" || prop.Name == "RefreshToken" || prop.Name == "RefreshTokenExpiryTime" || prop.Name == "ProvisionKey" || prop.Name == "SecretKey")
                    {
                        continue;
                    }
                    currentValues[prop.Name] = entry.CurrentValues[prop];
                }
                auditLog.NewValues = JsonSerializer.Serialize(currentValues);
            }
            
            this.AuditLogs.Add(auditLog);

            object entityToSerialize = entry.Entity;
            if (entry.Entity is User user)
            {
                entityToSerialize = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    PasswordHash = null,
                    Pin = null,
                    TenantId = user.TenantId,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    RefreshToken = null,
                    RefreshTokenExpiryTime = null,
                    RowVersion = user.RowVersion,
                    LastUpdated = user.LastUpdated
                };
            }

            var outboxMessage = new OutboxMessage
            {
                EventType = $"{entry.Entity.GetType().Name}_{entry.State}",
                Payload = System.Text.Json.JsonSerializer.Serialize(entityToSerialize, new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles }),
                CreatedAt = DateTime.UtcNow
            };
            
            if (tenantProp != null && tenantProp.CurrentValue != null)
            {
                outboxMessage.TenantId = tenantProp.CurrentValue.ToString() ?? string.Empty;
            }

            this.OutboxMessages.Add(outboxMessage);
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

public static class DictionaryHashCache
{
    private static readonly ConcurrentDictionary<string, string> _hashCache = new ConcurrentDictionary<string, string>();
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static string GetHash(Dictionary<string, object>? dict)
    {
        if (dict == null || dict.Count == 0) return string.Empty;
        
        // Serializamos para obtener una representación canónica
        string json = JsonSerializer.Serialize(dict, _jsonOptions);
        
        // Usamos cache para no recalcular el SHA256 de un mismo JSON
        return _hashCache.GetOrAdd(json, ComputeSha256);
    }

    private static string ComputeSha256(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}