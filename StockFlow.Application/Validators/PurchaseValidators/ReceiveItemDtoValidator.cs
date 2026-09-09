using FluentValidation;
using StockFlow.Application.DTOs.PurchaseOrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.PurchaseValidators
{
    public class ReceiveItemDtoValidator:AbstractValidator<ReceiveItemDto>
    {
        public ReceiveItemDtoValidator()
        {
            RuleFor(x => x.PurchaseOrderItemId).GreaterThan(0).WithMessage("Geçersiz sipariş kalemi ID'si.");
            RuleFor(x => x.ReceivedQuantity).GreaterThan(0).WithMessage("Alınan miktar 0'dan büyük olmalıdır.");
        }
    }
}
