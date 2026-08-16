using StockFlow.Domain.Common;
using StockFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Entities
{
    public class PurchaseOrder : BaseEntity
    {
        public required string PurchaseOrderNumber { get; set; }
        public PurchaseOrderStatus Status { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public DateTime? SentAt { get; set; }
        public DateTime? ReceivedAt { get; set; }

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }
}
