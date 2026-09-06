using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.OrderDtos
{
    public class ResultOrderDto
    {

        public required string OrderNumber { get; set; }
        public required string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int WarehouseId { get; set; }
        public required string WarehouseName { get; set; }
        public int CustomerId { get; set; }
        public required string CustomerFullName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhoneNumber { get; set; }
        public int CreatedByUserId { get; set; }           
        public required string CreatedByUserName { get; set; } 
        public required List<ResultOrderItemDto> Items { get; set; }

    }

}

