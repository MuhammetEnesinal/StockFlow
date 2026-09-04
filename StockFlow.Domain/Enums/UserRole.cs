using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Enums
{
    public enum UserRole
    {
        Admin, //Her türlü yetkiye sahip, tüm işlemleri yapabilir
        WarehouseManager, //Depo yönetimi ve stok takibi yapabilir, ürün ekleme, güncelleme ve silme işlemlerini gerçekleştirebilir
        WarehouseEmployee, //Depo çalışanı, stok giriş ve çıkış işlemlerini gerçekleştirebilir, ürünleri kontrol edebilir
        Viewer //Sadece okuma yetkisi olan kullanıcı
    }
}