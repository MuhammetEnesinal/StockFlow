using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.SupplierDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController(ISupplierService _supplierService) : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateSupplierDto dto)
        {
            var result = await _supplierService.CreateAsync(dto);
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _supplierService.GetAllAsync();
            return HandleResult(result);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _supplierService.GetByIdAsync(id);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id,UpdateSupplierDto dto)
        {
            var result = await _supplierService.UpdateAsync(id,dto);
            return HandleResult(result);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
           var result= await _supplierService.DeleteAsync(id);
           return HandleResult(result);
        }

     
    }
}
