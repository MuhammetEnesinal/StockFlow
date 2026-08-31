using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.ProductDtos
{
    public class ResultProductDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string SKU { get; set; }
        public decimal Price { get; set; }
        public int MinimumStockLevel { get; set; }
        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
    }
}
