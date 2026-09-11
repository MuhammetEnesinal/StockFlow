using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.PurchaseOrderDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrdersController(IPurchaseOrderService _purchaseOrderService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _purchaseOrderService.GetAllAsync();
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _purchaseOrderService.GetByIdAsync(id);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreatePurchaseOrderDto createPurchaseOrderDto)
        {
            var result = await _purchaseOrderService.CreateAsync(createPurchaseOrderDto);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPut("{id}/send")]
        public async Task<IActionResult> SendAsync(int id)
        {
            var result = await _purchaseOrderService.SendAsync(id);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPut("{id}/receive")]
        public async Task<IActionResult> ReceiveAsync(int id, ReceiveDto receiveDto)
        {
            var result = await _purchaseOrderService.ReceiveAsync(id, receiveDto);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelAsync(int id)
        {
            var result = await _purchaseOrderService.CancelAsync(id);
            return HandleResult(result);
        }

        [HttpGet("supplier/{supplierId}")]
        public async Task<IActionResult> GetBySupplierAsync(int supplierId)
        {
            var result = await _purchaseOrderService.GetBySupplierAsync(supplierId);
            return HandleResult(result);
        }
    }
}