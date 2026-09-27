using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.UsersDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService _userService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int pageNumber=1, [FromQuery] int pageSize=10)
        {
            pageSize=Math.Min(pageSize, 100);
            var users = await _userService.GetAllAsync(pageNumber, pageSize);
            return HandleResult(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return HandleResult(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateUserDto createUserDto)
        {
            var user = await _userService.CreateAsync(createUserDto);
            return HandleResult(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, UpdateUserDto updateUserDto)
        {
            var result = await _userService.UpdateAsync(id, updateUserDto);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _userService.DeleteAsync(id);
            return HandleResult(result);
        }

        [Authorize]
        [HttpPut("change-password/{id}")]
        public async Task<IActionResult> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
        {
            var result = await _userService.ChangePasswordAsync(id, changePasswordDto);
            return HandleResult(result);
        }
    }
}