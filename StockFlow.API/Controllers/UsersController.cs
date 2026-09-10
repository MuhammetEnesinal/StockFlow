using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.UsersDtos;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;

namespace StockFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService _userService) : BaseController
    {

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var users = await _userService.GetAllAsync();
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
            var result =await _userService.GetByIdAsync(id);
            return HandleResult(result);
        }


        [HttpPut("change-password/{id}")]
        public async Task<IActionResult> ChangePasswordAsync(int id,ChangePasswordDto changePasswordDto)
        {
            var result =await _userService.ChangePasswordAsync(id,changePasswordDto);
            return HandleResult(result);
        }
}

    }
