using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.StockDtos;
using StockFlow.Application.DTOs.TransferDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController(IStockService _stockService) : BaseController
    {
        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPost("in")]
        public async Task<IActionResult> StockInAsync(StockInDto dto)
        {
            var result = await _stockService.StockInAsync(dto);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPost("out")]
        public async Task<IActionResult> StockOutAsync(StockOutDto dto)
        {
            var result = await _stockService.StockOutAsync(dto);
            return HandleResult(result);
        }

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetStockByWarehouseAsync(int warehouseId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            pageSize = Math.Min(pageSize, 100);
            var result = await _stockService.GetStockByWarehouseAsync(warehouseId, pageNumber, pageSize);
            return HandleResult(result);
        }

        [HttpGet("{productId}/{warehouseId}/movements")]
        public async Task<IActionResult> GetMovementsAsync(int productId, int warehouseId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            pageSize = Math.Min(pageSize, 100);
            var result = await _stockService.GetMovementsAsync(productId, warehouseId, pageNumber, pageSize);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferAsync(TransferDto dto)
        {
            var result = await _stockService.TransferAsync(dto);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPost("transfer-batch")]
        public async Task<IActionResult> TransferBatchAsync(TransferBatchDto dto)
        {
            var result = await _stockService.TransferBatchAsync(dto);
            return HandleResult(result);
        }

        [HttpGet("warehouse/{warehouseId}/movements")]
        public async Task<IActionResult> GetMovementsByWarehouseAsync(int warehouseId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            pageSize = Math.Min(pageSize, 100);
            var result = await _stockService.GetMovementsByWarehouseAsync(warehouseId, pageNumber, pageSize);
            return HandleResult(result);
        }
    }
}