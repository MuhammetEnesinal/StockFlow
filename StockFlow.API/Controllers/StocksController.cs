using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.StockDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController(IStockService _stockService) : BaseController
    {

        [HttpPost("in")]
        public async Task<IActionResult> StockInAsync(StockInDto dto)
        {
            var result = await _stockService.StockInAsync(dto);
            return HandleResult(result);
        }

        [HttpPost("out")]
        public async Task<IActionResult> StockOutAsync(StockOutDto dto)
        {
            var result = await _stockService.StockOutAsync(dto);
            return HandleResult(result);
        }
        
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetStockByWarehouseAsync(int warehouseId)
        {
            var result = await _stockService.GetStockByWarehouseAsync(warehouseId);
            return HandleResult(result);
        }

        [HttpGet("{productId}/{warehouseId}/movements")]
        public async Task<IActionResult> GetMovementsAsync(int productId, int warehouseId)
        {
            var result = await _stockService.GetMovementsAsync(productId, warehouseId);
            return HandleResult(result);
        }

    }
}
