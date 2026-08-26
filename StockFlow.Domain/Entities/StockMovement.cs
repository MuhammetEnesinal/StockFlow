using StockFlow.Domain.Common;
using StockFlow.Domain.Enums;

namespace StockFlow.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }

    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public int? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    public Guid? TransferGroupId { get; set; }

    public int PerformedByUserId { get; set; }
    public User PerformedByUser { get; set; } = null!;
}