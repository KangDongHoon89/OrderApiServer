using Microsoft.AspNetCore.SignalR;
using OrderApiServer.Repositories;
using Serilog;

public class OrderHub : Hub
{
    private readonly IOrderRepository _orderRepository;

    public OrderHub(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task SendNewOrder(int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                Log.Warning($"주문을 찾을 수 없음: 주문 ID {id}");
                return;
            }

            await Clients.All.SendAsync("ReceiveNewOrder", order.Id, order.OrderId, order.MenuItem, order.Quantity, order.Status);
            Log.Information($"새 주문 전달 완료: 주문 ID {order.OrderId}, 메뉴 {order.MenuItem}, 수량 {order.Quantity}");
        }
        catch (Exception ex)
        {
            Log.Error($"새 주문 전달 실패! 주문 ID {id}, 오류: {ex.Message}");
        }
    }

    public async Task UpdateOrderStatus(int id, string newStatus)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order != null)
            {
                await _orderRepository.UpdateOrderStatusAsync(id, newStatus);

                // 모든 클라이언트에게 상태 변경 알림
                await Clients.All.SendAsync("ReceiveOrderStatusUpdate", order.Id, order.OrderId, newStatus);

                Log.Information($"주문 상태 변경됨: 주문 ID {order.OrderId}, 상태 {newStatus}");
            }
            else
            {
                Log.Warning($"주문을 찾을 수 없음: 주문 ID {id}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"주문 상태 업데이트 실패! 주문 ID {id}, 오류: {ex.Message}");
        }
    }
}