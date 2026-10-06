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
            var paged = await _auditlogRepository.GetAllAsync(pageNumber, pageSize);
            var result = _mapper.Map<PagedResult<ResultAuditLogDto>>(paged);

            return BaseResult<PagedResult<ResultAuditLogDto>>.Success(result);
        }

        public async Task<BaseResult<PagedResult<ResultAuditLogDto>>> GetByEntityNameAsync(string entityName, int pageNumber, int pageSize)
        {
            entityName = entityName.Trim();

            var paged = await _auditlogRepository.GetAllAsync(pageNumber, pageSize,
                filter: x => x.EntityName == entityName);
            var result = _mapper.Map<PagedResult<ResultAuditLogDto>>(paged);

            return BaseResult<PagedResult<ResultAuditLogDto>>.Success(result);
        }
        public async Task<BaseResult<PagedResult<ResultAuditLogDto>>> GetByUserIdAsync(int userId, int pageNumber, int pageSize)
        {
            var paged = await _auditlogRepository.GetAllAsync(pageNumber, pageSize,
                filter: x => x.PerformedByUserId == userId);
            var result = _mapper.Map<PagedResult<ResultAuditLogDto>>(paged);

            return BaseResult<PagedResult<ResultAuditLogDto>>.Success(result);
        }
    }
}
