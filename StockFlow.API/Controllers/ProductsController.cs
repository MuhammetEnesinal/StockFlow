using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.ProductDtos;
using StockFlow.Application.Interfaces.Services;


namespace StockFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductService _productService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int pageNumber=1, [FromQuery] int pageSize=10)
        {
            pageSize=Math.Min(pageSize, 100);
            var result = await _productService.GetAllAsync(pageNumber,pageSize);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateProductDto dto)
        {
            var result = await _productService.CreateAsync(dto);
            return HandleResult(result);

        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, UpdateProductDto dto)
        {
            var result = await _productService.UpdateAsync(id, dto);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _productService.DeleteAsync(id);
            return HandleResult(result);
        }
    }
}
