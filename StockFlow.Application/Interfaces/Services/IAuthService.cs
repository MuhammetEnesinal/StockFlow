using StockFlow.Application.Common;
using StockFlow.Application.DTOs.AuthDtos;


namespace StockFlow.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<BaseResult<LoginResultDto>> LoginAsync(LoginDto loginDto);
        Task<BaseResult<LoginResultDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<BaseResult<bool>> LogoutAsync();
    }
}
