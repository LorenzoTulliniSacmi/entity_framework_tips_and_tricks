using Exercise04_QueryOptimization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exercise04_QueryOptimization.Data;

public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }

    private readonly bool _enableLogging;

    // Costruttore parameterless per EF Core tools (migrations)
    public AppDbContext()
    {
    }

    // Costruttore con solo enableLogging
    public AppDbContext(bool enableLogging)
    {
        _enableLogging = enableLogging;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, bool enableLogging = false) : base(options)
    {
        _enableLogging = enableLogging;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configurazione per design-time (migrations)
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=ef_lab_optimization;Username=efuser;Password=efpass");
            // Alternativa SQLite: optionsBuilder.UseSqlite("Data Source=optimization.db");
        }

        if (_enableLogging)
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId);
        });
    }
}
