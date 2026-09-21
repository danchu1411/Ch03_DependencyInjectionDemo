using Ch03_DependencyInjectionDemo.Data;
using Ch03_DependencyInjectionDemo.Entities;
using Ch03_DependencyInjectionDemo.Repositories;

namespace Ch03_DependencyInjectionDemo.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly AppDbContext _context;

    public OrderService(
        IOrderRepository orderRepository,
        AppDbContext context)
    {
        _orderRepository = orderRepository;
        _context = context;
    }

    public async Task<bool> CreateOrderAsync(
        int productId,
        int quantity)
    {
        if (quantity <= 0)
            return false;

        var product = await _context.Products.FindAsync(productId);

        if (product == null)
            return false;

        if (product.Quantity < quantity)
            return false;

        var order = new Order
        {
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = product.Price,
            TotalAmount = product.Price * quantity,
            OrderDate = DateTime.Now
        };

        product.Quantity -= quantity;

        await _orderRepository.AddAsync(order);

        return true;
    }

    public Task<decimal> GetTotalRevenueAsync()
    {
        return _orderRepository.GetTotalRevenueAsync();
    }

    public Task<int> CountOrdersAsync(
        DateTime from,
        DateTime to)
    {
        return _orderRepository.CountByDateAsync(from, to);
    }
}