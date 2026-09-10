using StockFlow.Application.Common;
using StockFlow.Application.DTOs.PurchaseOrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface IPurchaseOrderService
    {
        Task<BaseResult<IEnumerable<ResultPurchaseOrderDto>>> GetAllAsync();
        Task<BaseResult<ResultPurchaseOrderDto>> GetByIdAsync(int id);
        Task<BaseResult<ResultPurchaseOrderDto>> CreateAsync(CreatePurchaseOrderDto createPurchaseOrderDto);
        Task<BaseResult<ResultPurchaseOrderDto>> SendAsync(int id);
        Task<BaseResult<ResultPurchaseOrderDto>> ReceiveAsync(int id, ReceiveDto receiveDto);
        Task<BaseResult<IEnumerable<ResultPurchaseOrderDto>>> GetBySupplierAsync(int supplierId);
        Task<BaseResult<bool>> CancelAsync(int id);
    }
}
