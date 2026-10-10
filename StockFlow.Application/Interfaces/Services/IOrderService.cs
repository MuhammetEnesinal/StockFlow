using StockFlow.Application.Common;
using StockFlow.Application.DTOs.OrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface IOrderService
    {

        Task<BaseResult<PagedResult<ResultOrderDto>>> GetAllAsync(int pageNumber, int pageSize);
        Task<BaseResult<ResultOrderDto>> GetByIdAsync(int id);
        Task<BaseResult<ResultOrderDto>> CreateAsync(CreateOrderDto dto);
        Task<BaseResult<ResultOrderDto>> ConfirmAsync(int id);
        Task<BaseResult<ResultOrderDto>> PrepareAsync(int id);
        Task<BaseResult<ResultOrderDto>> ShipAsync(int id);
        Task<BaseResult<ResultOrderDto>> DeliverAsync(int id);
        Task<BaseResult<bool>> CancelAsync(int id);
    }
}