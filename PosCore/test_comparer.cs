using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class TestDbContext : DbContext
{
    public DbSet<MyEntity> Entities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonOptions = new JsonSerializerOptions();
        
        var converter = new ValueConverter<Dictionary<string, object>, string>(
            v => JsonSerializer.Serialize(v, jsonOptions),
            v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, jsonOptions) ?? new Dictionary<string, object>()
        );

        var comparer = new ValueComparer<Dictionary<string, object>>(
            (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
            c => c == null ? 0 : JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
            c => c == null ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!
        );

        modelBuilder.Entity<MyEntity>()
            .Property(e => e.Data)
            .HasConversion(converter, comparer);
    }
}

public class MyEntity
{
    public int Id { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}
