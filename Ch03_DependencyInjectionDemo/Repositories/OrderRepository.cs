using Ch03_DependencyInjectionDemo.Data;
using Ch03_DependencyInjectionDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ch03_DependencyInjectionDemo.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public Task<List<Order>> GetAllAsync()
    {
        return _context.Orders
            .Include(o => o.Product)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.Orders
            .Select(o => (decimal?)o.TotalAmount)
            .SumAsync() ?? 0;
    }

    public Task<int> CountByDateAsync(DateTime from, DateTime to)
    {
        return _context.Orders
            .CountAsync(o =>
                o.OrderDate >= from &&
                o.OrderDate < to);
    }
}