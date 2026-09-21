using Ch03_DependencyInjectionDemo.Data;
using Ch03_DependencyInjectionDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ch03_DependencyInjectionDemo.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Product>> GetAllAsync()
    {
        return _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        return _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public Task<int> CountAsync()
    {
        return _context.Products.CountAsync();
    }
}