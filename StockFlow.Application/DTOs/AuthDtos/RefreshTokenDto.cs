using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.AuthDtos
{
    public class RefreshTokenDto
    {
       public required string RefreshToken {  get; set; }
    }
}
