using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OrderApiServer.Models;
using OrderApiServer.Repositories;
using Serilog;

namespace OrderApiServer.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<OrderHub> _orderHub;

        public OrderController(IOrderRepository orderRepository, IHubContext<OrderHub> orderHub)
        {
            _orderRepository = orderRepository;
            _orderHub = orderHub;
        }

        // 특정 상태의 주문 목록 조회 (GET /api/orders/pending)
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<Order>>> GetPendingOrders()
        {
            try
            {
                Log.Information("대기 중 또는 준비 중인 주문 조회 요청");

                var orders = await _orderRepository.GetOrdersAsync();
                var filteredOrders = orders
                    .Where(o => o.Status == "대기 중" || o.Status == "준비 중")
                    .OrderBy(o => o.CreatedAt)
                    .ToList();

                Log.Information($"{orders.Count}개의 대기 중/준비 중 주문 조회 완료");
                return Ok(filteredOrders);
            }
            catch (Exception ex)
            {
                Log.Error($"주문 목록 조회 실패! 오류: {ex.Message}");
                return StatusCode(500, "서버 오류로 인해 주문 목록을 조회할 수 없습니다.");
            }
        }

        // 주문 목록 조회 (GET /api/orders)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            try
            {
                Log.Information("주문 목록 조회 요청");

                var orders = await _orderRepository.GetOrdersAsync();

                Log.Information($"{orders.Count}개의 주문 조회 완료");
                return Ok(orders.OrderBy(o => o.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                Log.Error($"주문 목록 조회 실패! 오류: {ex.Message}");
                return StatusCode(500, "서버 오류로 인해 주문 목록을 조회할 수 없습니다.");
            }
        }

        // 특정 주문 조회 (GET /api/orders/{id})
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            try
            {
                Log.Information($"주문 조회 요청: 주문 ID {id}");

                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                {
                    Log.Warning($"주문을 찾을 수 없음: 주문 ID {id}");
                    return NotFound();
                }

                Log.Information($"주문 조회 성공: 주문 ID {id}");
                return Ok(order);
            }
            catch (Exception ex)
            {
                Log.Error($"주문 조회 실패! 주문 ID {id}, 오류: {ex.Message}");
                return StatusCode(500, "서버 오류로 인해 주문을 조회할 수 없습니다.");
            }
        }

        // 새로운 주문 추가 (POST /api/orders)
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            try
            {
                Log.Information($"새 주문 추가 요청: {order.OrderId} - {order.MenuItem} x {order.Quantity}");

                var orderId = order.OrderId ?? string.Empty;
                var existingOrder = await _orderRepository.GetOrderByOrderIdAsync(orderId);
                if (existingOrder != null)
                {
                    Log.Warning($"중복 주문 감지: 주문 ID {order.OrderId}");
                    return Conflict("이미 존재하는 주문입니다.");
                }

                await _orderRepository.AddOrderAsync(order);

                // SignalR을 통해 주문 접수 프로그램에 알림
                await _orderHub.Clients.All.SendAsync("ReceiveNewOrder", order.Id, order.OrderId, order.MenuItem, order.Quantity, order.Status, order.CreatedAt);

                Log.Information($"주문 추가 완료: {order.OrderId}");
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                Log.Error($"주문 추가 실패! 오류: {ex.Message}");
                return StatusCode(500, "서버 오류로 인해 주문을 추가할 수 없습니다.");
            }
        }

        // 주문 상태 업데이트 (PUT /api/orders/{id})
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order updatedOrder)
        {
            try
            {
                await _orderRepository.UpdateOrderStatusAsync(id, updatedOrder.Status);

                // 모든 클라이언트에게 상태 변경 알림
                await _orderHub.Clients.All.SendAsync("ReceiveOrderStatusUpdate", id, updatedOrder.OrderId, updatedOrder.Status);

                Log.Information($"주문 상태 변경 완료: 주문 ID {updatedOrder.OrderId}, 상태 {updatedOrder.Status}");
                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error($"주문 상태 변경 실패! 주문 ID {id}, 오류: {ex.Message}");
                return StatusCode(500, "서버 오류로 인해 주문 상태를 변경할 수 없습니다.");
            }
        }
    }
}
