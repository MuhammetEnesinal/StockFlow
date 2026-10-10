using BCrypt.Net;
using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.AuthDtos;
using StockFlow.Application.DTOs.UsersDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Domain.Entities;


namespace StockFlow.Application.Services
{
    public class AuthService(IGenericRepository<User> _userGenericRepository, ITokenService _tokenService, IValidator<LoginDto> _loginValidator, IValidator<ChangePasswordDto> _changePasswordValidator, IConfiguration _configuration, IUnitOfWork _unitOfWork, ICurrentUserService _currentUserService, IMapper _mapper) : IAuthService
    {
        private static readonly string _dummyPasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

        public async Task<BaseResult<LoginResultDto>> LoginAsync(LoginDto loginDto)
        {
            loginDto.Email = loginDto.Email.Trim();

            var validateResult = await _loginValidator.ValidateAsync(loginDto);
            if (!validateResult.IsValid)
            {
                return BaseResult<LoginResultDto>.Fail(validateResult.Errors);
            }

            var user = await _userGenericRepository.QueryForUpdate()
                        .Include(x => x.Warehouse)
                        .FirstOrDefaultAsync(x => x.Email == loginDto.Email);

            if (user == null)
            {
                BCrypt.Net.BCrypt.Verify(loginDto.Password, _dummyPasswordHash);
                return BaseResult<LoginResultDto>.Fail("Email veya şifre yanlış.", ResultErrorType.Unauthorized);
            }

            var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!isPasswordCorrect)
            {
                return BaseResult<LoginResultDto>.Fail("Email veya şifre yanlış.", ResultErrorType.Unauthorized);
            }

            if (!user.IsActive)
            {
                return BaseResult<LoginResultDto>.Fail("Hesabınız dondurulmuş veya kapatılmıştır.", ResultErrorType.Forbidden);
            }

            var token = _tokenService.GenerateToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            var resultUserDto = _mapper.Map<ResultUserDto>(user);
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"]!);

            var loginResult = new LoginResultDto
            {
                RefreshToken = refreshToken,
                Token = token,
                User = resultUserDto,
                RefreshTokenExpiresAt = user.RefreshTokenExpiresAt!.Value,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            await _unitOfWork.SaveChangesAsync();

            return BaseResult<LoginResultDto>.Success(loginResult);
        }

        public async Task<BaseResult<bool>> LogoutAsync()
        {
            var userId = _currentUserService.GetUserId();

            var user = await _userGenericRepository.QueryForUpdate()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return BaseResult<bool>.Fail("Kullanıcı Bulunamadı.", ResultErrorType.NotFound);
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            await _unitOfWork.SaveChangesAsync();
            return BaseResult<bool>.Success(true);
        }


        public async Task<BaseResult<LoginResultDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var user = await _userGenericRepository.QueryForUpdate()
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x => x.RefreshToken == refreshTokenDto.RefreshToken);

            if (user == null)
            {
                return BaseResult<LoginResultDto>.Fail("Geçersiz Refresh token.", ResultErrorType.Unauthorized);
            }

            if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
            {
                return BaseResult<LoginResultDto>.Fail("Refresh token süresi dolmuş.", ResultErrorType.Unauthorized);
            }

            if (!user.IsActive)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                await _unitOfWork.SaveChangesAsync();
                return BaseResult<LoginResultDto>.Fail("Hesabınız dondurulmuş veya kapatılmıştır.", ResultErrorType.Forbidden);
            }

            var acsessToken = _tokenService.GenerateToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            await _unitOfWork.SaveChangesAsync();

            var resultUserDto = _mapper.Map<ResultUserDto>(user);
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

        public async Task<BaseResult<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var validateResult = await _changePasswordValidator.ValidateAsync(changePasswordDto);
            if (!validateResult.IsValid)
            {
                return BaseResult<bool>.Fail(validateResult.Errors);
            }

            var userId = _currentUserService.GetUserId();

            var user = await _userGenericRepository.QueryForUpdate()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return BaseResult<bool>.Fail("Kullanıcı bulunamadı.", ResultErrorType.NotFound);
            }

            var isCorrect = BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash);
            if (!isCorrect)
            {
                return BaseResult<bool>.Fail("Mevcut şifre yanlış.", ResultErrorType.Unauthorized);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;

            await _unitOfWork.SaveChangesAsync();

            return BaseResult<bool>.Success(true);
        }
    }
}