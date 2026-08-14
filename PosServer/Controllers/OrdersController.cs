using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosDomain.Entities;
using PosApplication.Interfaces.Server;
using PosInfrastructure.Services.Server;
using PosDomain.Interfaces;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody, System.ComponentModel.DataAnnotations.Required] Order order)
        {
            if (order == null)
                return BadRequest("Payload de la orden es nulo.");

            var (isSuccess, message, orderId) = await _orderService.CreateOrUpdateOrderAsync(order);
            if (isSuccess)
            {
                return Ok(new { Message = message, ServerOrderId = orderId });
            }
            else
            {
                if (message.StartsWith("Conflicto de sincronización"))
                    return Conflict(new { Message = message, ServerOrderId = orderId });
                return BadRequest(new { Message = message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _orderService.GetOrdersAsync(page, pageSize);
            return Ok(new { data = result.data, page = result.page, pageSize = result.pageSize, total = result.total });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }
    }
}
