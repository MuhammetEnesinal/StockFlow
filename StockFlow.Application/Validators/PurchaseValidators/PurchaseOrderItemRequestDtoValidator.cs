using FluentValidation;
using StockFlow.Application.DTOs.PurchaseOrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.PurchaseValidators
{
    public class PurchaseOrderItemRequestDtoValidator:AbstractValidator<PurchaseOrderItemRequestDto>
    {

        public PurchaseOrderItemRequestDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Geçerli bir ürün seçilmelidir.");
            RuleFor(x => x.OrderedQuantity).GreaterThan(0).WithMessage("Sipariş miktarı 0'dan büyük olmalıdır.");
        }
    }
}
