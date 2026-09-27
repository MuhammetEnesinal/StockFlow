using StockFlow.Application.Common;
using StockFlow.Application.DTOs.PurchaseOrderDtos;


namespace StockFlow.Application.Interfaces.Services
{
    public interface IPurchaseOrderService
    {
        Task<BaseResult<PagedResult<ResultPurchaseOrderDto>>> GetAllAsync(int pageNumber,int pageSize);
        Task<BaseResult<ResultPurchaseOrderDto>> GetByIdAsync(int id);
        Task<BaseResult<ResultPurchaseOrderDto>> CreateAsync(CreatePurchaseOrderDto createPurchaseOrderDto);
        Task<BaseResult<ResultPurchaseOrderDto>> SendAsync(int id);
        Task<BaseResult<ResultPurchaseOrderDto>> ReceiveAsync(int id, ReceiveDto receiveDto);
        Task<BaseResult<PagedResult<ResultPurchaseOrderDto>>> GetBySupplierAsync(int supplierId, int pageNumber, int pageSize);
        Task<BaseResult<bool>> CancelAsync(int id);
    }
}
