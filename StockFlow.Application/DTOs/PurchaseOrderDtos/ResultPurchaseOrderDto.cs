using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.PurchaseOrderDtos
{
    public class ResultPurchaseOrderDto
    {
        public int Id { get; set; }
        public required string PurchaseOrderNumber { get; set; }
        public required string Status { get; set; }
        public int SupplierId { get; set; }
        public required string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public required string WarehouseName { get; set; }
        public int CreatedByUserId { get; set; }
        public required string CreatedByUserName { get; set; }
        public required string CreatedByEmployeeCode {  get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public required List<ResultPurchaseOrderItemDto> Items { get; set; }

    }
}
