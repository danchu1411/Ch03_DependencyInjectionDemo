using Ch03_DependencyInjectionDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ch03_DependencyInjectionDemo.Data;

/// <summary>
/// DbContext Code First. Khi dang ky bang AddDbContext, DbContext co lifetime = Scoped.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Du lieu goc - Code First seeding
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Laptop" },
            new Category { Id = 2, Name = "Smartphone" },
            new Category { Id = 3, Name = "Accessory" });

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Dell Latitude 5450", Price = 24_990_000m, Quantity = 12, CategoryId = 1 },
            new Product { Id = 2, Name = "MacBook Air M3", Price = 31_490_000m, Quantity = 7, CategoryId = 1 },
            new Product { Id = 3, Name = "Samsung Galaxy S24", Price = 19_990_000m, Quantity = 20, CategoryId = 2 },
            new Product { Id = 4, Name = "Logitech MX Master 3S", Price = 2_390_000m, Quantity = 35, CategoryId = 3 });

        base.OnModelCreating(modelBuilder);
    }
}
