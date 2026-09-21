using System.Text;
using Ch03_DependencyInjectionDemo.Data;
using Ch03_DependencyInjectionDemo.Demos;
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

        builder.Services.AddSingleton<IAppLogger, ConsoleLogger>();

        // Doi mot dong duoi day sang SmsNotificationService la doi toan bo kenh thong bao
        // ma KHONG can sua ProductService - do la loi ich cua Dependency Inversion.
        builder.Services.AddScoped<INotificationService, EmailNotificationService>();

        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IProductService, ProductService>();

        var host = builder.Build();

        // ---------- Code First: tao database neu chua co ----------
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            Console.WriteLine($"[DB] San sang. Hien co {await db.Products.CountAsync()} san pham.");
        }

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== PRN222 - Chapter 03: Dependency Injection in .NET =====");
            Console.WriteLine("1. Nguyen tac SOLID (S, O, L, I, D)");
            Console.WriteLine("2. Inversion of Control - co va khong co container");
            Console.WriteLine("3. Service Lifetimes - Transient / Scoped / Singleton");
            Console.WriteLine("4. 4 kieu DI: Constructor / Property / Method / Ambient Context");
            Console.WriteLine("5. Chay tat ca");
            Console.WriteLine("0. Thoat");
            Console.Write("Chon chuc nang: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": SolidAndPatternDemo.RunSolid(); break;
                case "2": SolidAndPatternDemo.RunIoC(); break;
                case "3": LifetimeDemo.Run(connectionString); break;
                case "4": await SolidAndPatternDemo.RunInjectionPatternsAsync(host.Services); break;
                case "5":
                    SolidAndPatternDemo.RunSolid();
                    SolidAndPatternDemo.RunIoC();
                    LifetimeDemo.Run(connectionString);
                    await SolidAndPatternDemo.RunInjectionPatternsAsync(host.Services);
                    break;
                case "0": return;
                default: Console.WriteLine("Lua chon khong hop le."); break;
            }
        }
    }
}
