using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public required string EntityName { get; set; }
        public int EntityId { get; set; }
        public required string Action { get; set; }

        public int PerformedByUserId { get; set; }
        public User PerformedByUser { get; set; } = null!;

        public string? Changes { get; set; }
    }
}
