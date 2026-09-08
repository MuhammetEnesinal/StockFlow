namespace StockFlow.Application.DTOs.TransferDtos
{
    public class TransferBatchDto
    {
        public required List<TransferDto> Transfers { get; set; }
    }
}