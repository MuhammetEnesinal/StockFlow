using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.OrderDtos;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Enums;

namespace StockFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IOrderService _orderService) : BaseController
    {

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var orders = await _orderService.GetAllAsync();
            return HandleResult(orders);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            return HandleResult(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateOrderDto createOrderDto)
        {
            var result = await _orderService.CreateAsync(createOrderDto);
            return HandleResult(result);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatusAsync(int id, [FromBody] OrderStatus newStatus)
        {
            var result = await _orderService.UpdateStatusAsync(id, newStatus);
            return HandleResult(result);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelAsync(int id)
        {
            var result = await _orderService.CancelAsync(id);
            return HandleResult(result);


        }
    }
}
