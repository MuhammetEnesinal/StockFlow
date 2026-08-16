using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Entities
{
    public class Category : BaseEntity
    {
        public required string Name { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
