using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.PurchaseOrderDtos
{
    public class ResultPurchaseOrderItemDto
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
