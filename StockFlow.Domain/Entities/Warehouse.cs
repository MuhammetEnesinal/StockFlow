using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Entities
{
    public class Warehouse :BaseEntity
    {
        public required string Name { get; set; }
        public string? Address { get; set; }
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    }
}
