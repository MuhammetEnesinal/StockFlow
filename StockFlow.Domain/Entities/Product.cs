using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Entities
{
    public class Product : BaseEntity
    {
        public required string Name { get; set; }
        public required string  SKU { get; set; } //Stock Keeping Unit

        public decimal Price { get; set; }
        public int MinimumStockLevel { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();

    }
}
