using Microsoft.AspNetCore.Http;
using StockFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Services
{
    public class CurrentUserService(IHttpContextAccessor _httpContextAccessor) : ICurrentUserService
    {
        public int GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            
            if(userIdClaim == null)
            {

                throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");
            }
         
            return int.Parse(userIdClaim.Value);
        }
    }
}
