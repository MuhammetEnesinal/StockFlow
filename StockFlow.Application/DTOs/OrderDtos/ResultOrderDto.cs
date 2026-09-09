namespace StockFlow.Application.DTOs.OrderDtos;

public class ResultOrderDto
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int CustomerId { get; set; }
    public required string CustomerFullName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public int CreatedByUserId { get; set; }
    public required string CreatedByUserName { get; set; }
    public required List<ResultOrderItemDto> Items { get; set; }
}