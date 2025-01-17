using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;

await using var ctx = new AppDbContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

var entities = new List<Foo>
{
    new(0, "EF", FooStatus.Open),
    new(0, "EF", FooStatus.Closed),
    // new(0, "EF", null),
};

ctx.Foos.AddRange(entities);
await ctx.SaveChangesAsync();

await ctx.BulkCopyAsync(
    new BulkCopyOptions(),
    entities.Select(x => x with { Source = "linq2db" })
);

var foos = await ctx.Foos.ToListAsync();

foreach (var foo in foos)
    Console.WriteLine(foo);


public class AppDbContext : DbContext
{
    public DbSet<Foo> Foos { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        const string connectionString =
            "Host=localhost;Port=5432;Database=linq2db;Username=postgres;Password=example;Include Error Detail=true";

        // default, string #1 & #2
        optionsBuilder.UseNpgsql(connectionString);

        // enum #1
        // optionsBuilder.UseNpgsql(connectionString, options => options.MapEnum<FooStatus>("foo_status"));

        // enum #2 & #3
        // var dataSource = new NpgsqlDataSourceBuilder(connectionString)
        //     .MapEnum<FooStatus>()
        //     .Build();
        // optionsBuilder.UseNpgsql(dataSource, options => options.MapEnum<FooStatus>());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Foo>()
            .Property(x => x.Status)
            // .HasConversion<string>() // string #1
            // .HasConversion(new EnumToStringConverter<FooStatus>()) // string #2
            ;

        // enum#3
        // modelBuilder.HasPostgresEnum<FooStatus>();
    }
}

public record Foo(
    int Id,
    string Source,
    FooStatus Status
);


public enum FooStatus
{
    Open,
    Closed
}
