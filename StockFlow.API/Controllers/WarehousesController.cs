using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.WarehouseDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController(IWarehouseService _warehouseService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _warehouseService.GetAllAsync();
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _warehouseService.GetByIdAsync(id);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateWarehouseDto dto)
        {
            var result = await _warehouseService.CreateAsync(dto);
            return HandleResult(result);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, UpdateWarehouseDto dto)
        {
            var result = await _warehouseService.UpdateAsync(id, dto);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _warehouseService.DeleteAsync(id);
            return HandleResult(result);
        }
    }
}
