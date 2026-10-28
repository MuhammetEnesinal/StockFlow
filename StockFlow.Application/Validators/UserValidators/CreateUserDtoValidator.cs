using FluentValidation;
using StockFlow.Application.DTOs.UsersDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Validators.UserValidators
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {

        public CreateUserDtoValidator()
        {
            RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Kullanıcı adı ve soyadı boş olamaz.")
            .MaximumLength(200).WithMessage("Kullanıcı adı ve soyadı 200 karakterden uzun olamaz.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email adresi boş bırakılamaz.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
                .MaximumLength(200).WithMessage("Email adresi 200 karakterden uzun olamaz.");


            RuleFor(x => x.PhoneNumber)
                .Length(11).WithMessage("Telefon numarası 11 haneli olmak zorundadır.")
                .Matches(@"^[0-9]+$").WithMessage("Telefon numarası sadece rakamlardan oluşmalıdır.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.EmployeeCode)
                .NotEmpty().WithMessage("Kullanıcı kodu boş bırakılamaz.")
                .MaximumLength(20).WithMessage("Kullanıcı kodu 20 karakterden uzun olamaz.");


            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("Depo ID 0'dan büyük olmalıdır.")
                .When(x => x.WarehouseId.HasValue);


            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş bırakılamaz.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .MaximumLength(36).WithMessage("Şifre 36 karakterden uzun olamaz.")
                .Matches(@"\p{Lu}").WithMessage("Şifre en az bir büyük harf içermelidir.")
                .Matches(@"\p{Ll}").WithMessage("Şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir.")
                .Matches(@"[^\p{L}\p{N}]").WithMessage("Şifre en az bir özel karakter içermelidir (harf ve rakam dışında bir karakter).");

            RuleFor(x => x.Role)
                .NotNull().WithMessage("Kullanıcı rolü boş bırakılamaz.")
                .IsInEnum().WithMessage("Geçersiz kullanıcı rolü.");
        }
    }
}
