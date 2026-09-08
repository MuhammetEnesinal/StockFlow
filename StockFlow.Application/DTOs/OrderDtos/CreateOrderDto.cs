namespace StockFlow.Application.DTOs.OrderDtos;

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public required List<OrderItemRequestDto> OrderItems { get; set; }
}