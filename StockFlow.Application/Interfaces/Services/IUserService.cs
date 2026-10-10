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
        Task<BaseResult<PagedResult<ResultUserDto>>> GetAllAsync(int pageNumber, int pageSize);
        Task<BaseResult<ResultUserDto>> GetByIdAsync(int id);
        Task<BaseResult<ResultUserDto>> CreateAsync(CreateUserDto createUserDto);

        Task<BaseResult<ResultUserDto>> UpdateAsync(int id, UpdateUserDto updateUserDto);
        Task<BaseResult<bool>> DeleteAsync(int id);
    }
}