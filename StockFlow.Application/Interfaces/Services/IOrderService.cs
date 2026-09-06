using StockFlow.Application.Common;
using StockFlow.Application.DTOs.OrderDtos;
using StockFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface IOrderService
    {

        Task<BaseResult<IEnumerable<ResultOrderDto>>> GetAllAsync();
        Task<BaseResult<ResultOrderDto>> GetByIdAsync(int id);
        Task<BaseResult<ResultOrderDto>> CreateAsync(CreateOrderDto dto);
        Task<BaseResult<ResultOrderDto>> UpdateStatusAsync(int id, OrderStatus newStatus);
        Task<BaseResult<bool>> CancelAsync(int id);
    }
}
