using StockFlow.Application.Common;
using StockFlow.Application.DTOs.AuditLogDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Services
{
    public interface IAuditLogService
    {
        Task<BaseResult<PagedResult<ResultAuditLogDto>>> GetAllAsync(int pageNumber, int pageSize);
        Task<BaseResult<PagedResult<ResultAuditLogDto>>> GetByEntityNameAsync(string entityName, int pageNumber, int pageSize);
        Task<BaseResult<PagedResult<ResultAuditLogDto>>> GetByUserIdAsync(int userId, int pageNumber, int pageSize);
    }
}
