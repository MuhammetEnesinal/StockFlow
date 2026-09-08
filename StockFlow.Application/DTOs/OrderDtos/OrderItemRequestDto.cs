namespace StockFlow.Application.DTOs.OrderDtos;

public class OrderItemRequestDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
}