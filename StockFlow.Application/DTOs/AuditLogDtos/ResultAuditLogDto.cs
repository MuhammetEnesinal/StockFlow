using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.AuditLogDtos
{
    public class ResultAuditLogDto
    {

        public int Id { get; set; }
        public required string EntityName { get; set; }
        public int EntityId { get; set; }

        public required string Action { get; set; }
        public int PerformedByUserId { get; set; }
        public required string PerformedByUserName { get; set; }
        public required string PerformedByEmployeeCode { get; set; }
        public string? Changes { get; set; }
        public DateTime CreateAtTime { get; set; }
    }
}
