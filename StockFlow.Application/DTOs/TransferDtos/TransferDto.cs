using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.TransferDtos
{
    public class TransferDto
    {
        public int ProductId { get; set; }
        public int SourceWarehouseId { get; set; }
        public int TargetWarehouseId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }
}
