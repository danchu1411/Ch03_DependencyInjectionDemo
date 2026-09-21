using Ch03_DependencyInjectionDemo.Entities;
using Ch03_DependencyInjectionDemo.Repositories;

namespace Ch03_DependencyInjectionDemo.Services;

public interface IProductService
{
    Guid InstanceId { get; }
    Task<List<Product>> GetAllAsync();
    Task CreateAsync(Product product, string notifyTo);
    Task ExportReportAsync(IAppLogger logger);   // Method Injection
}

/// <summary>
/// Minh hoa dong thoi 3 kieu injection (slide "Understanding Dependency Injection Patterns"):
///   - Constructor Injection : phu thuoc BAT BUOC  -> _repository, _notification
///   - Property  Injection   : phu thuoc TUY CHON  -> AuditLogger
///   - Method    Injection   : phu thuoc chi dung cho MOT thao tac -> ExportReportAsync(logger)
/// Kieu thu tu (Ambient Context) nam trong Services/AuditContext.cs.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly INotificationService _notification;

    // ---- 1. CONSTRUCTOR INJECTION ----
    public ProductService(IProductRepository repository, INotificationService notification)
    {
        _repository = repository;
        _notification = notification;
    }

    public Guid InstanceId { get; } = Guid.NewGuid();

    // ---- 2. PROPERTY INJECTION (phu thuoc tuy chon, co gia tri mac dinh) ----
    public IAppLogger? AuditLogger { get; set; }

    public Task<List<Product>> GetAllAsync() => _repository.GetAllAsync();

    public async Task CreateAsync(Product product, string notifyTo)
    {
        await _repository.AddAsync(product);

        // Ambient Context: lay thong tin nguoi dung hien tai ma khong can tham so
        AuditContext.Current.Write($"{AuditContext.Current.UserName} da them san pham '{product.Name}'");

        AuditLogger?.Log($"Da luu san pham #{product.Id} - {product.Name}");

        await _notification.SendAsync(notifyTo,
                                      "San pham moi",
                                      $"San pham {product.Name} vua duoc them vao kho.");
    }

    // ---- 3. METHOD INJECTION ----
    public async Task ExportReportAsync(IAppLogger logger)
    {
        var count = await _repository.CountAsync();
        logger.Log($"Bao cao: kho hien co {count} san pham.");
    }
}
