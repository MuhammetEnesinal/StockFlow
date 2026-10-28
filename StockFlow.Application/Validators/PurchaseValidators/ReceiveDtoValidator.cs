using FluentValidation;
using StockFlow.Application.DTOs.PurchaseOrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.PurchaseValidators
{
    public class ReceiveDtoValidator : AbstractValidator<ReceiveDto>
    {
        public ReceiveDtoValidator()
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage("Sipariş için en az bir ürün eklenmelidir.");

            RuleFor(x => x.Items)
                .Must(items => items.Count <= 100).WithMessage("Bir teslimatta en fazla 100 kalem olabilir.")
                .When(x => x.Items != null);

            RuleFor(x => x.Items)
                .Must(items => items.Select(i => i.PurchaseOrderItemId).Distinct().Count() == items.Count)
                .WithMessage("Aynı sipariş kalemi birden fazla gönderilemez.")
                .When(x => x.Items != null);
        }
    }
}