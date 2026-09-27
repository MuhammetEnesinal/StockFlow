using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.AuditLogDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;


namespace StockFlow.Application.Services
{
    public class AuditLogService(IGenericRepository<AuditLog> _auditlogRepository) : IAuditLogService
    {
        public async Task<BaseResult<PagedResult<ResultAuditLogDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var pagedAuditLogs = await _auditlogRepository.GetAllAsync(pageNumber, pageSize);
            var resultItems = new List<ResultAuditLogDto>();

            foreach (var auditLog in pagedAuditLogs.Items)
            {
                var resultAuditLog = new ResultAuditLogDto
                {
                    Id = auditLog.Id,
                    EntityName = auditLog.EntityName,
                    EntityId = auditLog.EntityId,
                    Action = auditLog.Action.ToString(),
                    PerformedByUserId = auditLog.PerformedByUserId,
                    PerformedByUserName = auditLog.PerformedByUserName,
                    PerformedByEmployeeCode = auditLog.PerformedByEmployeeCode,
                    Changes = auditLog.Changes,
                    CreateAtTime = auditLog.CreateAtTime,
                };

                resultItems.Add(resultAuditLog);
            }

            var result = new PagedResult<ResultAuditLogDto>
            {
                Items = resultItems,
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

            var resultItems = new List<ResultAuditLogDto>();

            foreach (var auditLog in auditLogs)
            {
                var resultAuditLog = new ResultAuditLogDto
                {
                    Id = auditLog.Id,
                    EntityName = auditLog.EntityName,
                    EntityId = auditLog.EntityId,
                    Action = auditLog.Action.ToString(),
                    PerformedByUserId = auditLog.PerformedByUserId,
                    PerformedByUserName = auditLog.PerformedByUserName,
                    PerformedByEmployeeCode = auditLog.PerformedByEmployeeCode,
                    Changes = auditLog.Changes,
                    CreateAtTime = auditLog.CreateAtTime,
                };

                resultItems.Add(resultAuditLog);
            }

            var result = new PagedResult<ResultAuditLogDto>
            {
                Items = resultItems,
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

            var resultItems = new List<ResultAuditLogDto>();

            foreach (var auditLog in auditLogs)
            {
                var resultAuditLog = new ResultAuditLogDto
                {
                    Id = auditLog.Id,
                    EntityName = auditLog.EntityName,
                    EntityId = auditLog.EntityId,
                    Action = auditLog.Action.ToString(),
                    PerformedByUserId = auditLog.PerformedByUserId,
                    PerformedByUserName = auditLog.PerformedByUserName,
                    PerformedByEmployeeCode = auditLog.PerformedByEmployeeCode,
                    Changes = auditLog.Changes,
                    CreateAtTime = auditLog.CreateAtTime,
                };

                resultItems.Add(resultAuditLog);
            }

            var result = new PagedResult<ResultAuditLogDto>
            {
                Items = resultItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return BaseResult<PagedResult<ResultAuditLogDto>>.Success(result);
        }


    }
}
