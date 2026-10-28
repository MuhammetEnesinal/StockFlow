using FluentValidation;
using StockFlow.Application.DTOs.OrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.OrderValidators
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {

            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Geçerli bir müşteri seçilmelidir.");

            RuleFor(x => x.OrderItems)
                .NotEmpty().WithMessage("Sipariş en az bir kalem içermelidir.");

            RuleFor(x => x.OrderItems)
                .Must(items => items.Count <= 100).WithMessage("Bir siparişte en fazla 100 kalem olabilir.")
                .When(x => x.OrderItems != null);
        }
    }
}