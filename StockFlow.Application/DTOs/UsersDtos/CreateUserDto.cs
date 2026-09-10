using StockFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.UsersDtos
{
    public class CreateUserDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FullName { get; set; }
        public required string EmployeeCode { get; set; }
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public int? WarehouseId { get; set; }
    }
}
