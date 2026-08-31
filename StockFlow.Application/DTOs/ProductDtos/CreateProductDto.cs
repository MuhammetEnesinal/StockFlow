namespace StockFlow.Application.DTOs.ProductDtos
{
    public class CreateProductDto
    {
        public required string Name { get; set; }
        public required string SKU { get; set; }
        public decimal Price { get; set; }
        public int MinimumStockLevel { get; set; }
        public int CategoryId { get; set; }
        public int? SupplierId { get; set; }
    }
}