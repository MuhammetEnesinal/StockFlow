using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.PurchaseOrderDtos
{
    public class ReceiveDto
    {
        public required List<ReceiveItemDto> Items { get; set; }   
    }
}
