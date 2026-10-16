using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StockFlow.API.ExceptionHandling
{
    public class GlobalExceptionHandler(
      ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
    {
        private static readonly JsonSerializerOptions _jsonOptions =
            new(JsonSerializerDefaults.Web)
            {
                Converters = { new JsonStringEnumConverter() }
            };

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            BaseResult<object> result;
            int statusCode;

            switch (exception)
            {
                case DbUpdateConcurrencyException:
                    _logger.LogWarning(
                        exception,
                        "Concurrency çakışması yakalandı.");

                    result = BaseResult<object>.Fail(
                        "Bu kayıt başka biri tarafından güncellendi, lütfen tekrar deneyin.",
                        ResultErrorType.Conflict);

                    statusCode = StatusCodes.Status409Conflict;
                    break;

                case DbUpdateException when exception.InnerException is SqlException { Number: 2601 or 2627 }:
                    _logger.LogWarning(
                        exception,
                        "Benzersizlik ihlali yakalandı.");

                    result = BaseResult<object>.Fail(
                        "Bu kayıt zaten mevcut.",
                        ResultErrorType.Conflict);

                    statusCode = StatusCodes.Status409Conflict;
                    break;

                case DbUpdateException when exception.InnerException is SqlException { Number: 547 }:
                    _logger.LogWarning(
                        exception,
                        "FOREIGN KEY veya CHECK kısıtı ihlali yakalandı.");

                    result = BaseResult<object>.Fail(
                        "Bu kayıt başka kayıtlarla ilişkili olduğu veya bir kuralı ihlal ettiği için işlem yapılamadı.",
                        ResultErrorType.Conflict);

                    statusCode = StatusCodes.Status409Conflict;
                    break;

                case DbUpdateException:
                    _logger.LogError(
                        exception,
                        "Veritabanı güncelleme hatası oluştu.");

                    result = BaseResult<object>.Fail(
                        "Veritabanı işlemi gerçekleştirilemedi.",
                        ResultErrorType.Conflict);

                    statusCode = StatusCodes.Status409Conflict;
                    break;

                case UnauthorizedAccessException:
                    _logger.LogWarning(
                        exception,
                        "Kimlik bilgisi bulunamadı.");

                    result = BaseResult<object>.Fail(
                        "Kimlik doğrulaması gerekli.",
                        ResultErrorType.Unauthorized);

                    statusCode = StatusCodes.Status401Unauthorized;
                    break;

                default:
                    _logger.LogError(
                        exception,
                        "Beklenmeyen bir hata oluştu.");

                    result = BaseResult<object>.Fail(
                        "Beklenmeyen bir hata oluştu.");

                    statusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(
                result,
                _jsonOptions,
                cancellationToken);

            return true;
        }
    }
}