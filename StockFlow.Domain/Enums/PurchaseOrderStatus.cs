using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Enums
{
    public enum PurchaseOrderStatus
    {
        Draft,
        Sent,
        PartiallyReceived,
        Received,
        Cancelled
    }
}