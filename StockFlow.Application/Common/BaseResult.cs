using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Common
{
    public class BaseResult<T>
    {
        public T? Data { get; set; }
        public IEnumerable<ResultError>? Errors { get; set; }
        public ResultErrorType? ErrorType { get; set; }

        public bool IsSuccessful=>Errors==null || !Errors.Any();
        public bool IsFailure => !IsSuccessful;

        public static BaseResult<T> Success(T? data) => new() { Data = data };
        
        public static BaseResult<T> Success() => new();
        public static BaseResult<T> Fail(string errorMessage, ResultErrorType errorType = ResultErrorType.BusinessRule) => new() { Errors = new[] { new ResultError(null, errorMessage) }, ErrorType = errorType };

    }
}