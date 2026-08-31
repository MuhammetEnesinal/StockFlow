using FluentValidation;
using StockFlow.Application.DTOs.SupplierDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.SupplierValidators
{
    public class UpdateSupplierDtoValidator:AbstractValidator<UpdateSupplierDto>
    {
        public UpdateSupplierDtoValidator() {

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tedarikçi adı boş olamaz.")
                .MaximumLength(200).WithMessage("Tedarikçi adı 200 karakterden uzun olamaz.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
                .MaximumLength(200).WithMessage("Email adresi 200 karakterden uzun olamaz.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.PhoneNumber)
                .Length(11).WithMessage("Telefon numarası 11 haneli olmak zorundadır.")
                .Matches(@"^\d+$").WithMessage("Telefon numarası sadece rakamlardan oluşmalıdır.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));


        }

    }
}
