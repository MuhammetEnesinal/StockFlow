using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.AuthDtos;
using StockFlow.Application.DTOs.UsersDtos;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService _authService) : BaseController
    {
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            return HandleResult(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);
            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var result = await _authService.LogoutAsync();
            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var result = await _authService.ChangePasswordAsync(changePasswordDto);
            return HandleResult(result);
        }
    }
}