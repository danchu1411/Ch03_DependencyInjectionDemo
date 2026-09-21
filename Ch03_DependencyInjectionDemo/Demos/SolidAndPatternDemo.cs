using Ch03_DependencyInjectionDemo.Entities;
using Ch03_DependencyInjectionDemo.Services;
using Ch03_DependencyInjectionDemo.Solid;
using Microsoft.Extensions.DependencyInjection;

namespace Ch03_DependencyInjectionDemo.Demos;

public static class SolidAndPatternDemo
{
    /// <summary>Slide "Understanding The SOLID Principles" va 5 demo kem theo.</summary>
    public static void RunSolid()
    {
        var items = new List<Product>
        {
            new() { Name = "Laptop", Price = 20_000_000m, Quantity = 2 },
            new() { Name = "Mouse",  Price = 500_000m,    Quantity = 5 }
        };

        Console.WriteLine();
        Console.WriteLine("===== S - SINGLE RESPONSIBILITY =====");
        var total = new InvoiceCalculator().CalculateTotal(items);
        Console.WriteLine($"  {new InvoiceFileWriter().Write(total)}");
        Console.WriteLine($"  {new InvoiceMailer().Send("tamttt14@fe.edu.vn", total)}");
        Console.WriteLine("  => Doi cach ghi file khong lam anh huong toi cach tinh tien.");

        Console.WriteLine();
        Console.WriteLine("===== O - OPEN/CLOSED =====");
        var policies = new List<IDiscountPolicy>
        {
            new RegularCustomerDiscount(),
            new VipCustomerDiscount(),
            new StudentDiscount()          // them moi ma khong sua lop nao
        };
        foreach (var p in policies)
            Console.WriteLine($"  {p.Name,-8}: giam {p.GetDiscount(total):N0} tren {total:N0}");

        Console.WriteLine();
        Console.WriteLine("===== L - LISKOV SUBSTITUTION =====");
        var badRect = new RectangleBad { Width = 5, Height = 4 };
        var badSquare = new SquareBad { Width = 5, Height = 4 };
        Console.WriteLine($"  RectangleBad(5x4).Area = {badRect.Area}  (dung nhu mong doi)");
        Console.WriteLine($"  SquareBad(5x4).Area    = {badSquare.Area}  (SAI: dat Height da doi luon Width)");
        IShape[] shapes = { new Rectangle(5, 4), new Square(5) };
        foreach (var s in shapes)
            Console.WriteLine($"  {s.Name,-10}: Area = {s.Area}  (thay the duoc cho nhau an toan)");

        Console.WriteLine();
        Console.WriteLine("===== I - INTERFACE SEGREGATION =====");
        IPrinter printer = new SimplePrinter();
        Console.WriteLine($"  {printer.Print("Bang diem PRN222")}");
        var office = new OfficeMachine();
        Console.WriteLine($"  {office.Print("Hop dong")}");
        Console.WriteLine($"  {office.Scan("C:\\scan\\hop-dong.pdf")}");
        Console.WriteLine($"  {office.Fax("0236-9999999")}");
        Console.WriteLine("  => SimplePrinter khong bi ep cai dat Scan/Fax.");

        Console.WriteLine();
        Console.WriteLine("===== D - DEPENDENCY INVERSION =====");
        var production = new OrderProcessor(new SqlServerOrderStore());
        Console.WriteLine($"  {production.Process("DH-2026-001")}");

        var testStore = new InMemoryOrderStore();
        var underTest = new OrderProcessor(testStore);
        Console.WriteLine($"  {underTest.Process("DH-TEST-001")}");
        Console.WriteLine($"  => Unit test kiem tra duoc: testStore.Orders.Count = {testStore.Orders.Count}, " +
                          "khong can database that.");
    }

    /// <summary>
    /// Slide "Understanding Inversion of Control (IoC)" va "IoC Pattern Demo".
    /// So sanh: tu tao phu thuoc (control) vs de container tao (inversion of control).
    /// </summary>
    public static void RunIoC()
    {
        Console.WriteLine();
        Console.WriteLine("===== INVERSION OF CONTROL =====");

        Console.WriteLine("1) KHONG co IoC - lop tu quyet dinh phu thuoc cua minh:");
        var bad = new OrderProcessorBad();
        Console.WriteLine($"   {bad.Process("DH-001")}");
        Console.WriteLine("   Muon doi sang kho khac phai SUA MA NGUON cua OrderProcessorBad.");

        Console.WriteLine();
        Console.WriteLine("2) CO IoC - quyen quyet dinh chuyen ra ngoai (container):");
        var services = new ServiceCollection();
        services.AddSingleton<IOrderStore, SqlServerOrderStore>();
        services.AddTransient<OrderProcessor>();

        using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<OrderProcessor>();
        Console.WriteLine($"   {processor.Process("DH-002")}");
        Console.WriteLine("   Doi kho luu tru = doi MOT dong dang ky, khong dong vao OrderProcessor.");
    }

    /// <summary>
    /// Slide "Understanding Dependency Injection Patterns":
    /// Constructor / Property / Method / Ambient Context.
    /// </summary>
    public static async Task RunInjectionPatternsAsync(IServiceProvider rootProvider)
    {
        Console.WriteLine();
        Console.WriteLine("===== 4 KIEU DEPENDENCY INJECTION =====");

        using var scope = rootProvider.CreateScope();
        var sp = scope.ServiceProvider;

        var service = (ProductService)sp.GetRequiredService<IProductService>();

        // 1. Constructor Injection - da xay ra khi container dung ProductService
        Console.WriteLine("1) CONSTRUCTOR INJECTION");
        Console.WriteLine("   IProductRepository va INotificationService da duoc tiem qua constructor.");

        // 2. Property Injection - phu thuoc tuy chon, gan sau khi tao
        Console.WriteLine();
        Console.WriteLine("2) PROPERTY INJECTION (phu thuoc tuy chon)");
        Console.WriteLine("   Chua gan AuditLogger -> khong ghi log:");
        var p1 = new Product { Name = "Keyboard Keychron K2", Price = 2_190_000m, Quantity = 10, CategoryId = 3 };
        await service.CreateAsync(p1, "kho@fe.edu.vn");

        Console.WriteLine("   Da gan AuditLogger -> co ghi log:");
        service.AuditLogger = sp.GetRequiredService<IAppLogger>();
        var p2 = new Product { Name = "Monitor Dell U2723QE", Price = 14_500_000m, Quantity = 4, CategoryId = 3 };
        await service.CreateAsync(p2, "kho@fe.edu.vn");

        // 3. Method Injection - phu thuoc chi can cho mot thao tac
        Console.WriteLine();
        Console.WriteLine("3) METHOD INJECTION");
        await service.ExportReportAsync(sp.GetRequiredService<IAppLogger>());

        // 4. Ambient Context - truy cap qua thuoc tinh tinh
        Console.WriteLine();
        Console.WriteLine("4) AMBIENT CONTEXT");
        Console.WriteLine("   Mac dinh (DefaultAuditContext):");
        var p3 = new Product { Name = "USB-C Hub", Price = 890_000m, Quantity = 25, CategoryId = 3 };
        await service.CreateAsync(p3, "kho@fe.edu.vn");

        Console.WriteLine("   Doi sang FakeAuditContext (dung khi unit test):");
        var fake = new FakeAuditContext("tamttt14");
        AuditContext.Current = fake;
        var p4 = new Product { Name = "Webcam Logitech C920", Price = 1_690_000m, Quantity = 8, CategoryId = 3 };
        await service.CreateAsync(p4, "kho@fe.edu.vn");
        Console.WriteLine($"   FakeAuditContext da bat duoc {fake.Entries.Count} ban ghi:");
        foreach (var entry in fake.Entries)
            Console.WriteLine($"     - {entry}");

        AuditContext.Current = DefaultAuditContext.Instance;   // tra lai trang thai ban dau

        Console.WriteLine();
        Console.WriteLine("Danh sach san pham hien co trong SQL Server:");
        foreach (var p in await service.GetAllAsync())
            Console.WriteLine($"   #{p.Id,-3} {p.Name,-28} {p.Price,15:N0}  SL:{p.Quantity,-4} {p.Category?.Name}");
    }
}
