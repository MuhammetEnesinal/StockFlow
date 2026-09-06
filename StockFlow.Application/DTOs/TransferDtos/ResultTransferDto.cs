public class ResultTransferDto
{
    public required string ProductName { get; set; }
    public int SourceWarehouseId { get; set; }
    public required string SourceWarehouseName { get; set; }
    public int SourceRemainingQuantity { get; set; }
    public int TargetWarehouseId { get; set; }
    public required string TargetWarehouseName { get; set; }
    public int TargetNewQuantity { get; set; }
    public int TransferredQuantity { get; set; }
}