using StockFlow.Application.DTOs.UsersDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.AuthDtos
{
    public class LoginResultDto
    {
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
        public required ResultUserDto User { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
