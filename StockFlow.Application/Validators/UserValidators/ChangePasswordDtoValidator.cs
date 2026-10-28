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
                .NotEmpty().WithMessage("Mevcut şifre boş bırakılamaz.")
                .MaximumLength(200).WithMessage("Mevcut şifre 200 karakterden uzun olamaz.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifre boş bırakılamaz.")
                .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır.")
                .MaximumLength(36).WithMessage("Yeni şifre 36 karakterden uzun olamaz.")
                .Matches(@"\p{Lu}").WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
                .Matches(@"\p{Ll}").WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir.")
                .Matches(@"[^\p{L}\p{N}]").WithMessage("Yeni şifre en az bir özel karakter içermelidir (harf ve rakam dışında bir karakter).")
                .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifre mevcut şifreyle aynı olamaz.");
        }
    }
}