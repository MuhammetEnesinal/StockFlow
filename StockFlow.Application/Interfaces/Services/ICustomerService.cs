using StockFlow.Application.Common;
using StockFlow.Application.DTOs.CustomerDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<BaseResult<PagedResult<ResultCustomerDto>>> GetAllAsync(int pageNumber,int pageSize);
        Task<BaseResult<ResultCustomerDto>> GetByIdAsync(int id);
        Task<BaseResult<ResultCustomerDto>> CreateAsync(CreateCustomerDto createCustomerDto);
        Task<BaseResult<ResultCustomerDto>> UpdateAsync(int id, UpdateCustomerDto updateCustomerDto);
        Task<BaseResult<bool>> DeleteAsync(int id);
    }
}