using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.DTOs.UsersDtos
{
    public class ResultUserDto
    {

        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string EmployeeCode { get; set; }
        public string? PhoneNumber { get; set; }
        public required string Role { get; set; }
        public bool IsActive { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
    }
}
