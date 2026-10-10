using FluentValidation;
using StockFlow.Application.DTOs.UsersDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.UserValidators
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Mevcut şifre boş bırakılamaz.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifre boş bırakılamaz.")
                .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır.")
                .MaximumLength(200).WithMessage("Yeni şifre 200 karakterden uzun olamaz.")
                .Matches(@"[A-Z]").WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]").WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir.")
                .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifre mevcut şifreyle aynı olamaz.");
        }
    }
}