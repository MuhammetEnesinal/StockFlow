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
using StockFlow.Domain.Enums;


namespace StockFlow.Application.Services
{
    public class AuthService(IGenericRepository<User> _userGenericRepository,ITokenService _tokenService,IValidator<LoginDto> _loginValidator,IConfiguration _configuration, IGenericRepository<AuditLog> _auditLogRepository, IUnitOfWork _unitOfWork,ICurrentUserService _currentUserService) : IAuthService
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

            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            _userGenericRepository.Update(user);

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
                RefreshToken = refreshToken,
                Token = token,
                User = resultUserDto,
                RefreshTokenExpiresAt = user.RefreshTokenExpiresAt!.Value,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

         

            var loginAuditLog = new AuditLog
            {
                EntityName = "User",
                Action = AuditAction.Login,
                PerformedByUserId = user.Id,
                PerformedByUserName = user.FullName,
                PerformedByEmployeeCode = user.EmployeeCode,
                CreateAtTime = DateTime.UtcNow,
                Changes = null
            };
            await _auditLogRepository.AddAsync(loginAuditLog);
            await _unitOfWork.SaveChangesAsync();

            return BaseResult<LoginResultDto>.Success(loginResult);
        }

        public async Task<BaseResult<bool>> LogoutAsync()
        {
            var userId = _currentUserService.GetUserId();

            var user = await _userGenericRepository.Query()
                .FirstOrDefaultAsync(x=>x.Id == userId);

            if(user == null)
            {
                return BaseResult<bool>.Fail("Kullanıcı Bulunamadı.", ResultErrorType.NotFound);
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            _userGenericRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return BaseResult<bool>.Success(true);
        }


        public async Task<BaseResult<LoginResultDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var user = await _userGenericRepository.Query()
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x => x.RefreshToken == refreshTokenDto.RefreshToken);

            if (user == null) {

                return BaseResult<LoginResultDto>.Fail("Geçersiz Refresh token.", ResultErrorType.Unauthorized);
            }

            if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
            {
                return BaseResult<LoginResultDto>.Fail("Refresh token süresi dolmuş.", ResultErrorType.Unauthorized);
            }

            var acsessToken = _tokenService.GenerateToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            _userGenericRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();


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

            var loginResultDto = new LoginResultDto
            {
                RefreshToken = refreshToken,
                Token = acsessToken,
                User = resultUserDto,
                RefreshTokenExpiresAt = user.RefreshTokenExpiresAt!.Value,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)

            };
            
            return BaseResult<LoginResultDto>.Success(loginResultDto);
        }
    }
}
