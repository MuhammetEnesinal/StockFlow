using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.OrderDtos;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Enums;

namespace StockFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IOrderService _orderService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize=10)
        {
            pageSize = Math.Min(pageSize, 100);
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
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatusAsync(int id, [FromBody] OrderStatus newStatus)
        {
            var result = await _orderService.UpdateStatusAsync(id, newStatus);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelAsync(int id)
        {
            var result = await _orderService.CancelAsync(id);
            return HandleResult(result);
        }
    }
}