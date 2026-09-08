
namespace StockFlow.Application.DTOs.OrderDtos;
public class ResultOrderItemDto
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public int WarehouseId { get; set; }
    public required string WarehouseName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}