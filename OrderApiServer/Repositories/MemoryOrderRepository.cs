using OrderApiServer.Models;

namespace OrderApiServer.Repositories
{
    public class MemoryOrderRepository : IOrderRepository
    {
        // 메모리 저장소
        private readonly List<Order> _orders = new List<Order>(); 

        public Task<List<Order>> GetOrdersAsync()
        {
            return Task.FromResult(_orders);
        }

        public Task<Order?> GetOrderByIdAsync(int id)
        {
            return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
        }

        public Task<Order?> GetOrderByOrderIdAsync(string orderId)
        {
            return Task.FromResult(_orders.FirstOrDefault(o => o.OrderId == orderId));
        }

        public Task AddOrderAsync(Order order)
        {
            order.Id = _orders.Count > 0 ? _orders.Max(o => o.Id) + 1 : 1;
            order.CreatedAt = DateTime.Now;
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task UpdateOrderStatusAsync(int id, string status)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.Status = status;
            }
            return Task.CompletedTask;
        }
    }
}