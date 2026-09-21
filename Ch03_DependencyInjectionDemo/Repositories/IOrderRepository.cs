using Ch03_DependencyInjectionDemo.Entities;

namespace Ch03_DependencyInjectionDemo.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<List<Order>> GetAllAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<int> CountByDateAsync(DateTime from, DateTime to);
}