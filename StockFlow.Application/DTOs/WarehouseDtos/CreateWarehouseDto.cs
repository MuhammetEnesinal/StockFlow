using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.WarehouseDtos
{
    public class CreateWarehouseDto
    {
        public required string Name { get; set; }
        public string? Address { get; set; }
    }
}
