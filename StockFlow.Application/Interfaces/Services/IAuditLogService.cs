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
        Task<BaseResult<IEnumerable<ResultAuditLogDto>>> GetAllAsync();
        Task<BaseResult<IEnumerable<ResultAuditLogDto>>> GetByEntityNameAsync(string entityName);
        Task<BaseResult<IEnumerable<ResultAuditLogDto>>> GetByUserIdAsync(int userId);
    }
}
