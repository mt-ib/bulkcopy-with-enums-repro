using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

await using var ctx = new AppDbContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

var entities = new List<Foo>
{
    new(0, "EF", FooStatus.Open),
    new(0, "EF", FooStatus.Closed),
    new(0, "EF", null),
};

ctx.Foos.AddRange(entities);
await ctx.SaveChangesAsync();

await ctx.BulkCopyAsync(
    new BulkCopyOptions(),
    entities.Select(x => x with { Source = "linq2db" })
);



public class AppDbContext : DbContext
{
    public DbSet<Foo> Foos { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        const string connectionString =
            "Host=localhost;Port=5432;Database=linq2db;Username=postgres;Password=example;Include Error Detail=true";

        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Foo>()
            .Property(x => x.Status)
            .HasConversion(new EnumToStringConverter<FooStatus>());
    }
}

public record Foo(
    int Id,
    string Source,
    FooStatus? Status
);


public enum FooStatus
{
    Open,
    Closed
}
