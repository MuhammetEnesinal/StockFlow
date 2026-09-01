using FluentValidation;
using StockFlow.Application.DTOs.StockDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.StockValidators
{
    public class StockInDtoValidator:AbstractValidator<StockInDto>
    {
        public StockInDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Ürün ID 0'dan büyük olmalıdır.");

            RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Depo ID 0'dan büyük olmalıdır.");

            RuleFor(x => x.Quantity).NotEmpty().WithMessage("Miktar boş bırakılamaz.")
                .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");

            RuleFor(x => x.Note).MaximumLength(500).WithMessage("Not 500 karakterden uzun olamaz.")
                .When(x => !string.IsNullOrEmpty(x.Note));
        }
    }
}
