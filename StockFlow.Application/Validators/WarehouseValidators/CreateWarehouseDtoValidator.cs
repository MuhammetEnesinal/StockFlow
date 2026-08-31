using FluentValidation;
using StockFlow.Application.DTOs.WarehouseDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.WarehouseValidators
{
    public class CreateWarehouseDtoValidator:AbstractValidator<CreateWarehouseDto>
    {

        public CreateWarehouseDtoValidator()
        {
            RuleFor(x=>x.Name).NotEmpty().WithMessage("Depo adı boş olamaz.")
                .MaximumLength(200).WithMessage("Depo adı 200 karakterden fazla olamaz.");
            RuleFor(x => x.Address).MaximumLength(500).WithMessage("Depo adresi 500 karakterden fazla olamaz.")
                  .When(x => !string.IsNullOrEmpty(x.Address));
        }
    }
}
