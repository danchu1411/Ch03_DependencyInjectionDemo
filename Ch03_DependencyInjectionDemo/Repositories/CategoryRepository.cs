using Ch03_DependencyInjectionDemo.Data;
using Ch03_DependencyInjectionDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ch03_DependencyInjectionDemo.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Category>> GetAllAsync()
    {
        return _context.Categories
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        return _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return;

        bool hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
        {
            throw new InvalidOperationException(
                "Không thể xóa Category vì đang có Product thuộc Category này.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}