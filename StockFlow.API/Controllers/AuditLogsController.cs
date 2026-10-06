using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Services;

namespace StockFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController(IAuditLogService _auditLogService) : BaseController
    {

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var auditLogs = await _auditLogService.GetAllAsync(pageNumber, pageSize);
            return HandleResult(auditLogs);
        }


        [HttpGet("entity/{entityName}")]
        public async Task<IActionResult> GetByEntityNameAsync(string entityName, [FromQuery] int pageNumber=1 , [FromQuery] int pageSize=10)
        {
            var auditLogs = await _auditLogService.GetByEntityNameAsync(entityName, pageNumber, pageSize);
            return HandleResult(auditLogs);
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserIdAsync(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var auditLogs = await _auditLogService.GetByUserIdAsync(userId, pageNumber, pageSize);
            return HandleResult(auditLogs);
        }
    }
}
