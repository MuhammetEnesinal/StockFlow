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
        Task<BaseResult<ResultCategoryDto>> CreateAsync(CreateCategoryDto createCategoryDto);
        Task<BaseResult<ResultCategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto);

        Task<BaseResult<bool>> DeleteAsync(int id);
    }
}
