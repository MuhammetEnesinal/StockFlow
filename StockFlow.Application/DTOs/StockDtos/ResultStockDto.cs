using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.StockDtos
{
    public class ResultStockDto
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public required string WarehouseName { get; set; }
        public int Quantity { get; set; }
    }
}
