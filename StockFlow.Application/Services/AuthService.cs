using BCrypt.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.AuthDtos;
using StockFlow.Application.DTOs.CustomerDtos;
using StockFlow.Application.DTOs.UsersDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Domain.Entities;


namespace StockFlow.Application.Services
{
    public class AuthService(IGenericRepository<User> _userGenericRepository,ITokenService _tokenService,IValidator<LoginDto> _loginValidator,IConfiguration _configuration) : IAuthService
    {
        public async Task<BaseResult<LoginResultDto>> LoginAsync(LoginDto loginDto)
        {
            loginDto.Email = loginDto.Email.Trim();
            loginDto.Password = loginDto.Password.Trim();

            var validateResult =await _loginValidator.ValidateAsync(loginDto);
            if (!validateResult.IsValid) {

                return BaseResult<LoginResultDto>.Fail(validateResult.Errors);
            
            }

            var user = await _userGenericRepository.Query()
                        .Include(x => x.Warehouse)
                        .FirstOrDefaultAsync(x=>x.Email == loginDto.Email);

            if (user == null) {
                return BaseResult<LoginResultDto>.Fail("Email veya şifre yanlış.",ResultErrorType.Unauthorized);
            }

            if (user.IsActive == false)
            {
                return BaseResult<LoginResultDto>.Fail("Hesabınız dondurulmuş veya kapatılmıştır.", ResultErrorType.Forbidden);
            }

            var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);


            if (!isPasswordCorrect)
            {
                return BaseResult<LoginResultDto>.Fail("Email veya şifre yanlış.", ResultErrorType.Unauthorized);
            }
            var token = _tokenService.GenerateToken(user);

            var resultUserDto = new ResultUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                EmployeeCode = user.EmployeeCode,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                WarehouseId = user.WarehouseId,
                WarehouseName = user.Warehouse?.Name
            };
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"]!);

            var loginResult = new LoginResultDto
            {
                Token = token,
                User = resultUserDto,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            return BaseResult<LoginResultDto>.Success(loginResult);
        }
    }
}
