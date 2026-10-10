using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.OrderDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IOrderService _orderService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var orders = await _orderService.GetAllAsync(pageNumber, pageSize);
            return HandleResult(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            return HandleResult(order);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateOrderDto createOrderDto)
        {
            var result = await _orderService.CreateAsync(createOrderDto);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmAsync(int id)
        {
            var result = await _orderService.ConfirmAsync(id);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPost("{id}/prepare")]
        public async Task<IActionResult> PrepareAsync(int id)
        {
            var result = await _orderService.PrepareAsync(id);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPost("{id}/ship")]
        public async Task<IActionResult> ShipAsync(int id)
        {
            var result = await _orderService.ShipAsync(id);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPost("{id}/deliver")]
        public async Task<IActionResult> DeliverAsync(int id)
        {
            var result = await _orderService.DeliverAsync(id);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelAsync(int id)
        {
            var result = await _orderService.CancelAsync(id);
            return HandleResult(result);
        }
    }
}