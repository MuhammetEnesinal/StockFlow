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

    }
}
