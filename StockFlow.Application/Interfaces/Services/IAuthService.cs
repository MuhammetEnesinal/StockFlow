using StockFlow.Application.Common;
using StockFlow.Application.DTOs.AuthDtos;


namespace StockFlow.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<BaseResult<LoginResultDto>> LoginAsync(LoginDto loginDto);

    }
}
