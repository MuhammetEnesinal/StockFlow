using FluentValidation;
using StockFlow.Application.DTOs.TransferDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.TransferValidators
{
    public class TransferDtoValidator:AbstractValidator<TransferDto>
    {

        public TransferDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Ürün Id'si 0'dan büyük olmalıdır.");
            
            RuleFor(x => x.SourceWarehouseId).GreaterThan(0).WithMessage("Kaynak Depo Id'si 0'dan büyük olmalıdır.");
            
            RuleFor(x => x.TargetWarehouseId).GreaterThan(0).WithMessage("Hedef Depo Id'si 0'dan büyük olmalıdır.");

            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");

            RuleFor(x => x.Note).MaximumLength(500).WithMessage("Not alanı 500 karakterden uzun olamaz.")
                .When(x => !string.IsNullOrEmpty(x.Note));

            RuleFor(x => x.TargetWarehouseId)
                .NotEqual(x => x.SourceWarehouseId)
                .WithMessage("Kaynak ve hedef depo aynı olamaz.");
        }
    }
}
