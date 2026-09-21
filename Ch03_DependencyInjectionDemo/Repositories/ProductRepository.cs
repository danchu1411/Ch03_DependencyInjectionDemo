using Ch03_DependencyInjectionDemo.Data;
using Ch03_DependencyInjectionDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ch03_DependencyInjectionDemo.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    // Constructor Injection: DbContext duoc container dua vao
    public ProductRepository(AppDbContext context) => _context = context;

    public Guid InstanceId { get; } = Guid.NewGuid();

    public Task<List<Product>> GetAllAsync()
        => _context.Products.Include(p => p.Category).AsNoTracking().ToListAsync();

    public Task<Product?> GetByIdAsync(int id)
        => _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public Task<int> CountAsync() => _context.Products.CountAsync();
}
