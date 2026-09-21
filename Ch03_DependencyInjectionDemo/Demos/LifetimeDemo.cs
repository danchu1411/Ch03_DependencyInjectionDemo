using Ch03_DependencyInjectionDemo.Data;
using Ch03_DependencyInjectionDemo.Repositories;
using Ch03_DependencyInjectionDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ch03_DependencyInjectionDemo.Demos;

/// <summary>
/// Slide "Service Lifetimes" va "The Microsoft.Extensions.DependencyInjection Container".
/// Dung InstanceId (Guid) de nhin thay ro khi nao container tao instance moi.
/// </summary>
public static class LifetimeDemo
{
    public static void Run(string connectionString)
    {
        Console.WriteLine();
        Console.WriteLine("===== SERVICE LIFETIMES =====");
        Console.WriteLine("Transient : moi lan yeu cau -> mot instance moi");
        Console.WriteLine("Scoped    : mot instance cho moi scope (trong web = moi HTTP request)");
        Console.WriteLine("Singleton : mot instance duy nhat cho ca ung dung");
        Console.WriteLine();

        var services = new ServiceCollection();

        services.AddTransient<TransientService>();
        services.AddScoped<ScopedService>();
        services.AddSingleton<SingletonService>();

        // AddDbContext mac dinh dang ky voi lifetime Scoped
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IProductRepository, ProductRepository>();

        using var provider = services.BuildServiceProvider();

        for (var scopeNumber = 1; scopeNumber <= 2; scopeNumber++)
        {
            Console.WriteLine($"--- SCOPE {scopeNumber} ---");
            using var scope = provider.CreateScope();
            var sp = scope.ServiceProvider;

            for (var call = 1; call <= 2; call++)
            {
                var transient = sp.GetRequiredService<TransientService>();
                var scoped = sp.GetRequiredService<ScopedService>();
                var singleton = sp.GetRequiredService<SingletonService>();
                var repo = sp.GetRequiredService<IProductRepository>();

                Console.WriteLine($"  Lan lay #{call}");
                Console.WriteLine($"    Transient : {Short(transient.Id)}");
                Console.WriteLine($"    Scoped    : {Short(scoped.Id)}");
                Console.WriteLine($"    Singleton : {Short(singleton.Id)}");
                Console.WriteLine($"    Repository: {Short(repo.InstanceId)}  (Scoped, chua DbContext)");
            }
            Console.WriteLine();
        }

        Console.WriteLine("Ket luan:");
        Console.WriteLine("  - Transient doi ID moi lan lay.");
        Console.WriteLine("  - Scoped giu nguyen ID trong cung mot scope, doi khi sang scope khac.");
        Console.WriteLine("  - Singleton giu nguyen ID trong toan bo vong doi ung dung.");
        Console.WriteLine();
        Console.WriteLine("CANH BAO 'captive dependency': dang ky Singleton ma phu thuoc Scoped");
        Console.WriteLine("se giu DbContext song mai mai -> ro ri bo nho va du lieu cu. .NET se nem");
        Console.WriteLine("InvalidOperationException khi ValidateScopes duoc bat (mac dinh o moi truong Development).");
    }

    private static string Short(Guid id) => id.ToString()[..8];

    // Cac lop don gian chi de quan sat lifetime
    public class TransientService { public Guid Id { get; } = Guid.NewGuid(); }
    public class ScopedService { public Guid Id { get; } = Guid.NewGuid(); }
    public class SingletonService { public Guid Id { get; } = Guid.NewGuid(); }
}
