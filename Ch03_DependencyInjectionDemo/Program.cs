using System.Text;
using Ch03_DependencyInjectionDemo.Data;
using Ch03_DependencyInjectionDemo.Repositories;
using Ch03_DependencyInjectionDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ch03_DependencyInjectionDemo;

/// <summary>
/// CHUONG 03 - Dependency Injection in .NET
/// Su dung Generic Host + Microsoft.Extensions.DependencyInjection,
/// tang du lieu la EF Core Code First tren SQL Server.
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "PRN222 - Chapter 03: Dependency Injection";

        var builder = Host.CreateApplicationBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DiDemoDB")
                               ?? throw new InvalidOperationException("Thieu connection string 'DiDemoDB'.");

        // ---------- DANG KY DICH VU VAO CONTAINER ----------
        // AddDbContext: lifetime Scoped
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();

        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IProductService, ProductService>();

        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderService, OrderService>();


        var host = builder.Build();

        // ---------- Db First ----------
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Console.WriteLine(
                $"[DB] San sang. Hien co {await db.Products.CountAsync()} san pham.");
        }

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== CATEGORY MANAGEMENT =====");
            Console.WriteLine("1. Danh sách Categories");
            Console.WriteLine("2. Thêm Category");
            Console.WriteLine("3. Sửa Category");
            Console.WriteLine("4. Xóa Category");
            Console.WriteLine("0. Thoat");
            Console.Write("Chon chuc nang: ");

            switch (Console.ReadLine()?.Trim())
            {

            }
        }
    }

    static async Task ShowCategoriesAsync(
    ICategoryService service)
    {
        var categories = await service.GetAllAsync();

        Console.WriteLine();
        Console.WriteLine("===== CATEGORIES =====");
        Console.WriteLine("ID\tName");

        foreach (var category in categories)
        {
            Console.WriteLine(
                $"{category.Id}\t{category.Name}");
        }
    }
}
