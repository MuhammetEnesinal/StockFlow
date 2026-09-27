using StockFlow.Application.Common;
using StockFlow.Application.DTOs.StockDtos;
using StockFlow.Application.DTOs.TransferDtos;
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
        Task<BaseResult<ResultTransferDto>> TransferAsync(TransferDto transferDto);
        Task<BaseResult<PagedResult<ResultStockDto>>> GetStockByWarehouseAsync(int warehouseId, int pageNumber, int pageSize);
        Task<BaseResult<PagedResult<ResultStockMovementDto>>> GetMovementsAsync(int productId, int warehouseId, int pageNumber, int pageSize);
        Task<BaseResult<IEnumerable<ResultTransferDto>>> TransferBatchAsync(TransferBatchDto transferBatchDto);
        Task<BaseResult<PagedResult<ResultStockMovementDto>>> GetMovementsByWarehouseAsync(int warehouseId, int pageNumber, int pageSize);
    }
}
