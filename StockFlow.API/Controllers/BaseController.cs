using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.Common;

namespace StockFlow.API.Controllers
{
    public class BaseController : ControllerBase
    {
        protected IActionResult HandleResult<T>(BaseResult<T> result)
        {
            if (result.IsSuccessful)
                return Ok(result);

            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(result),
                ResultErrorType.Conflict => Conflict(result),
                ResultErrorType.Validation => UnprocessableEntity(result),
                ResultErrorType.Unauthorized => Unauthorized(result),
                ResultErrorType.Forbidden => StatusCode(403, result),
                _ => BadRequest(result)
            };
        }
    }
}