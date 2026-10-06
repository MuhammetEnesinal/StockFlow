using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.CategoryDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(ICategoryService _categoryService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize=10)
        {
            var result = await _categoryService.GetAllAsync(pageNumber,pageSize);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateCategoryDto dto)
        {
            var result =await _categoryService.CreateAsync(dto);
            return HandleResult(result);

        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateAsync(id, dto);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return HandleResult(result);
        }
    }
}
