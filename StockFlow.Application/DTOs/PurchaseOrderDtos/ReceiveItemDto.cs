using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.PurchaseOrderDtos
{
    public class ReceiveItemDto
    {
        public int PurchaseOrderItemId { get; set; }
        public int ReceivedQuantity { get; set; }
    }
}
