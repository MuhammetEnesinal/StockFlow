using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.PurchaseOrderDtos
{
    public class PurchaseOrderItemRequestDto
    {
        public int ProductId { get; set; }
        public int OrderedQuantity { get; set; }
    }
}
