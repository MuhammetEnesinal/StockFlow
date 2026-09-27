using StockFlow.Application.Common;
using StockFlow.Application.DTOs.SupplierDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface ISupplierService
    {
        Task<BaseResult<PagedResult<ResultSupplierDto>>> GetAllAsync(int pageNumber,int pageSize);
        Task<BaseResult<ResultSupplierDto>> GetByIdAsync(int id);

        Task<BaseResult<ResultSupplierDto>> UpdateAsync(int id, UpdateSupplierDto dto);
        Task<BaseResult<ResultSupplierDto>> CreateAsync(CreateSupplierDto dto);
        Task<BaseResult<bool>> DeleteAsync(int id);
    }
}
