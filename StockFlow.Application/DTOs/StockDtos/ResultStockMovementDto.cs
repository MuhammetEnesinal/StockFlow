

namespace StockFlow.Application.DTOs.StockDtos
{
    public class ResultStockMovementDto
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public required string WarehouseName { get; set; }
        public required string Type { get; set; }
        public  string? Note { get; set; }
        public int Quantity { get; set; }
        public int PerformedByUserId { get; set; }
        public required string PerformedByUserName { get; set; }
        public DateTime? CreateAtTime { get; set; }

    }
}
