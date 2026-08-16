using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Common
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreateAtTime { get; set; }
        public DateTime? UpdateAtTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
