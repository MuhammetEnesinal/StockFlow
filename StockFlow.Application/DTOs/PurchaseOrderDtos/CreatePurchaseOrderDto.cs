
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.PurchaseOrderDtos
{
    public class CreatePurchaseOrderDto
    {
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public required List<PurchaseOrderItemRequestDto> Items { get; set; }
    }
}
