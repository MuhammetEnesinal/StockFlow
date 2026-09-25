using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.CustomerDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController(ICustomerService _customerService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var customers = await _customerService.GetAllAsync();
            return HandleResult(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            return HandleResult(customer);
        }

        [Authorize(Roles = "Admin,WarehouseManager,WarehouseEmployee")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateCustomerDto dto)
        {
            var result = await _customerService.CreateAsync(dto);
            return HandleResult(result);

        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, UpdateCustomerDto dto)
        {
            var result = await _customerService.UpdateAsync(id, dto);
            return HandleResult(result);
        }

        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _customerService.DeleteAsync(id);
            return HandleResult(result);
        }


    }
}
