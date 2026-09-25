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
        public async Task<BaseResult<IEnumerable<ResultAuditLogDto>>> GetAllAsync()
        {
            var auditLogs = await _auditlogRepository.GetAllAsync();
            var resultItems = new List<ResultAuditLogDto>();

            foreach (var auditLog in auditLogs) {

                var resultAuditLog = new ResultAuditLogDto
                {
                    Id = auditLog.Id,
                    EntityName = auditLog.EntityName,
                    EntityId = auditLog.EntityId,
                    Action=auditLog.Action.ToString(),
                    PerformedByUserId=auditLog.PerformedByUserId,
                    PerformedByUserName=auditLog.PerformedByUserName,
                    PerformedByEmployeeCode=auditLog.PerformedByEmployeeCode,
                    Changes=auditLog.Changes,
                    CreateAtTime=auditLog.CreateAtTime,
                };

                resultItems.Add(resultAuditLog);


            }

            return BaseResult< IEnumerable< ResultAuditLogDto >>.Success(resultItems);
        }

        public async Task<BaseResult<IEnumerable<ResultAuditLogDto>>> GetByEntityNameAsync(string entityName)
        {
            entityName = entityName.Trim();
            var auditLogs = await _auditlogRepository.Query()
              .Where(x => x.EntityName == entityName)
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

            return BaseResult<IEnumerable<ResultAuditLogDto>>.Success(resultItems);
        }

        public  async Task<BaseResult<IEnumerable<ResultAuditLogDto>>> GetByUserIdAsync(int userId)
        {
            var auditLogs = await _auditlogRepository.Query()
              .Where(x => x.PerformedByUserId == userId)
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

            return BaseResult<IEnumerable<ResultAuditLogDto>>.Success(resultItems);

        }


    }
}
