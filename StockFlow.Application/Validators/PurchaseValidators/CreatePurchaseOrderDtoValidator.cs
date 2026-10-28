using FluentValidation;
using StockFlow.Application.DTOs.PurchaseOrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.PurchaseValidators
{
    public class CreatePurchaseOrderDtoValidator : AbstractValidator<CreatePurchaseOrderDto>
    {
        public CreatePurchaseOrderDtoValidator()
        {
            RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("Geçerli bir tedarikçi seçilmelidir.");
            RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Geçerli bir depo seçilmelidir.");
            RuleFor(x => x.PurchaseOrderItems).NotEmpty().WithMessage("Sipariş için en az bir ürün eklenmelidir.");

            RuleFor(x => x.PurchaseOrderItems)
                .Must(items => items.Count <= 100).WithMessage("Bir satın alma siparişinde en fazla 100 kalem olabilir.")
                .When(x => x.PurchaseOrderItems != null);
        }
    }
}