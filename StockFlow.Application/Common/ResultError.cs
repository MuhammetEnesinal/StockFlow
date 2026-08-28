using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Common
{
    public class ResultError
    {
        public string? PropertyName { get; set; }
        public string ErrorMessage { get; set; }

        public ResultError(string? propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }
    }
}
