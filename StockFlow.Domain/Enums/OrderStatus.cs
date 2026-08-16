

namespace StockFlow.Domain.Enums
{
    public enum OrderStatus
    {
        Pendindg, //Sipariş oluşturuldu, onay bekliyor
        Confirmed, // Sipariş onaylandı, hazırlık aşamasına geçildi
        Preparing, //jSipariş hazırlanıyor, paketleme ve sevkiyat için hazır hale getiriliyor
        Shipped, //Sipariş kargoya verildi, teslimat sürecinde
        Delivered, //Sipariş teslim edildi, müşteri tarafından alındı
        Cancelled //Sipariş iptal edildi, işlem durduruldu
    }
}
