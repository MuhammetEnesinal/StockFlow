using StockFlow.Application.Common;
using StockFlow.Application.DTOs.CategoryDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<BaseResult<IEnumerable<ResultCategoryDto>>> GetAllAsync();
        Task<BaseResult<ResultCategoryDto>> GetByIdAsync(int id);
        Task<BaseResult<object>> CreateAsync(CreateCategoryDto createCategoryDto);
        Task<BaseResult<object>> UpdateAsync(int id, UpdateCategoryDto dto);

        Task<BaseResult<object>> DeleteAsync(int id);
    }
}
