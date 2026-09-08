using FluentValidation;
using StockFlow.Application.DTOs.OrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.OrderValidators
{
    public class OrderItemRequestDtoValidator:AbstractValidator<OrderItemRequestDto>
    {
        public OrderItemRequestDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Ürün ID'si 0'dan büyük olmalıdır.");
            RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Depo ID'si 0'dan büyük olmalıdır.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");
        }
    }
}
