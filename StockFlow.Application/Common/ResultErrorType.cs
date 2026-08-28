using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Common
{
    public enum ResultErrorType
    {
        NotFound,
        Validation,
        BusinessRule,
        Conflict,
        Unauthorized,
        Forbidden
    }
}
