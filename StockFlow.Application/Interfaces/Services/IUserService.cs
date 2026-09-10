using StockFlow.Application.Common;
using StockFlow.Application.DTOs.UsersDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<BaseResult<IEnumerable<ResultUserDto>>> GetAllAsync();
        Task<BaseResult<ResultUserDto>> GetByIdAsync(int id);
        Task<BaseResult<ResultUserDto>> CreateAsync(CreateUserDto createUserDto);

        Task<BaseResult<ResultUserDto>> UpdateAsync(int id, UpdateUserDto updateUserDto);
        Task<BaseResult<bool>> DeleteAsync(int id);
        Task<BaseResult<bool>> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto);
    }
}
