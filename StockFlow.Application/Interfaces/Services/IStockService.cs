using StockFlow.Application.Common;
using StockFlow.Application.DTOs.StockDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface IStockService
    {
        Task<BaseResult<ResultStockDto>> StockInAsync(StockInDto stockInDto);
        Task<BaseResult<ResultStockDto>> StockOutAsync(StockOutDto stockOutDto);
        Task<BaseResult<IEnumerable<ResultStockDto>>> GetStockByWarehouseAsync(int warehouseId);
        Task<BaseResult<IEnumerable<ResultStockMovementDto>>> GetMovementsAsync(int productId,int warehouseId);

    }
}
