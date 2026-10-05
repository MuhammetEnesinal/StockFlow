using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.AuditLogDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;


namespace StockFlow.Application.Services
{
    public class AuditLogService(IGenericRepository<AuditLog> _auditlogRepository, IMapper _mapper) : IAuditLogService
    {
        public async Task<BaseResult<PagedResult<ResultAuditLogDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var pagedAuditLogs = await _auditlogRepository.GetAllAsync(pageNumber, pageSize);

            var mappedItems = _mapper.Map<List<ResultAuditLogDto>>(pagedAuditLogs.Items);

            var result = new PagedResult<ResultAuditLogDto>
            {
                Items = mappedItems,
                PageNumber = pagedAuditLogs.PageNumber,
                PageSize = pagedAuditLogs.PageSize,
                TotalCount = pagedAuditLogs.TotalCount
            };

            return BaseResult<PagedResult<ResultAuditLogDto>>.Success(result);
        }

        public async Task<BaseResult<PagedResult<ResultAuditLogDto>>> GetByEntityNameAsync(string entityName, int pageNumber, int pageSize)
        {
            entityName = entityName.Trim();

            var query = _auditlogRepository.Query()
                .Where(x => x.EntityName == entityName);

            var totalCount = await query.CountAsync();

            var auditLogs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedItems = _mapper.Map<List<ResultAuditLogDto>>(auditLogs);

            var result = new PagedResult<ResultAuditLogDto>
            {
                Items = mappedItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return BaseResult<PagedResult<ResultAuditLogDto>>.Success(result);
        }

        public async Task<BaseResult<PagedResult<ResultAuditLogDto>>> GetByUserIdAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _auditlogRepository.Query()
                .Where(x => x.PerformedByUserId == userId);

            var totalCount = await query.CountAsync();

            var auditLogs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedItems = _mapper.Map<List<ResultAuditLogDto>>(auditLogs);

            var result = new PagedResult<ResultAuditLogDto>
            {
                Items = mappedItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return BaseResult<PagedResult<ResultAuditLogDto>>.Success(result);
        }
    }
}