namespace Ch03_DependencyInjectionDemo.Services;

public interface IOrderService
{
    Task<bool> CreateOrderAsync(
        int productId,
        int quantity);

    Task<decimal> GetTotalRevenueAsync();

    Task<int> CountOrdersAsync(
        DateTime from,
        DateTime to);
}