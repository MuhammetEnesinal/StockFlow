using StockFlow.Application.Common;
using StockFlow.Application.DTOs.ProductDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<BaseResult<IEnumerable<ResultProductDto>>> GetAllAsync();
        Task<BaseResult<ResultProductDto>> GetByIdAsync(int id);
        Task<BaseResult<ResultProductDto>> CreateAsync(CreateProductDto dto);
        Task<BaseResult<ResultProductDto>> UpdateAsync(int id,UpdateProductDto dto);
        Task<BaseResult<bool>> DeleteAsync(int id);
    }
}
