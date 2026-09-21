using Ch03_DependencyInjectionDemo.Entities;

namespace Ch03_DependencyInjectionDemo.Repositories;

/// <summary>
/// Truu tuong hoa tang truy cap du lieu.
/// Slide "Dependency Inversion Principle": module cap cao phu thuoc vao abstraction,
/// khong phu thuoc truc tiep vao EF Core.
/// </summary>
public interface IProductRepository
{
    /// <summary>Ma dinh danh cua instance - dung de quan sat service lifetime.</summary>
    Guid InstanceId { get; }

    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task<int> CountAsync();
}
