

namespace StockFlow.Domain.Enums
{
    public enum StockMovementType
    {
        Purchase, //satın alma girişi
        Sale, //satış çıktısı
        TransferIn, //transfer girişi
        TransferOut, //transfer çıktısı
        Adjustment, // manuel düzeltme
        Return //iade

    }
}
