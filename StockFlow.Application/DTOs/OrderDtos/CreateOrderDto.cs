using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.OrderDtos
{
    public class CreateOrderDto
    {
        public int WarehouseId { get; set; }
        public int CustomerId { get; set; }
        public required List<OrderItemRequestDto> OrderItems { get; set; }
    }
}
