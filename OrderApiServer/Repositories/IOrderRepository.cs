using OrderApiServer.Models;

namespace OrderApiServer.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order?> GetOrderByOrderIdAsync(string orderId);
        Task AddOrderAsync(Order order);
        Task UpdateOrderStatusAsync(int id, string status);
    }
}