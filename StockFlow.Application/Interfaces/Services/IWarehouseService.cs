using StockFlow.Application.Common;
using StockFlow.Application.DTOs.WarehouseDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface IWarehouseService
    {

        Task<BaseResult<IEnumerable<ResultWarehouseDto>>> GetAllAsync();
        Task<BaseResult<ResultWarehouseDto>> GetByIdAsync(int id);
        Task<BaseResult<ResultWarehouseDto>> CreateAsync(CreateWarehouseDto dto);
        Task<BaseResult<ResultWarehouseDto>> UpdateAsync(int id,UpdateWarehouseDto dto);
        Task<BaseResult<bool>> DeleteAsync(int id);
    }
}
