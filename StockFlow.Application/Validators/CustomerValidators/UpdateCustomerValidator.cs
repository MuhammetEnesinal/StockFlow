using FluentValidation;
using StockFlow.Application.DTOs.CustomerDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.CustomerValidators
{
    public class UpdateCustomerValidator:AbstractValidator<UpdateCustomerDto>
    {
        public UpdateCustomerValidator()
        {
            RuleFor(x => x.FullName)
              .NotEmpty().WithMessage("Müşteri adı boş olamaz.")
              .MaximumLength(200).WithMessage("Müşteri adı 200 karakterden uzun olamaz.");

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
