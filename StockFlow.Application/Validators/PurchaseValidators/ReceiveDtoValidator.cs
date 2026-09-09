using FluentValidation;
using StockFlow.Application.DTOs.PurchaseOrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.PurchaseValidators
{
    public class ReceiveDtoValidator:AbstractValidator<ReceiveDto>
    {
        public ReceiveDtoValidator()
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage("Sipariş için en az bir ürün eklenmelidir.");
        }
    }
}
