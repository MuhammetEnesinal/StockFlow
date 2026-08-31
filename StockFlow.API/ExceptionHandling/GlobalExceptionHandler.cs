using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StockFlow.API.ExceptionHandling
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
    {

        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {

            BaseResult<object> result;
            int statusCode;

            switch (exception)
            {

                case DbUpdateConcurrencyException:
                    _logger.LogWarning(exception, "Concurrency çakışması yakaladı.");
                    result = BaseResult<object>.Fail("Bu kayıt başka biri tarafından güncellendi, lütfen tekrar deneyin.", ResultErrorType.Conflict);
                    statusCode = StatusCodes.Status409Conflict;
                    break;

                case DbUpdateException:
                    _logger.LogWarning(exception, "Veritabanı kısıtı ihlali yakalandı.");
                    result = BaseResult<object>.Fail("Bu kayıt, sistemde zaten var olan bir kayıtla çakışıyor.", ResultErrorType.Conflict);
                    statusCode = StatusCodes.Status409Conflict;
                    break;
                default:
                    _logger.LogError(exception, "Beklenmeyen bir hata oluştu.");
                    result = BaseResult<object>.Fail("Beklenmeyen bir hata oluştu.");
                    statusCode = StatusCodes.Status500InternalServerError;
                    break;


            }
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType="application/json";
            await httpContext.Response.WriteAsJsonAsync(result, _jsonOptions, cancellationToken);
            return true;
        }
    }
}
