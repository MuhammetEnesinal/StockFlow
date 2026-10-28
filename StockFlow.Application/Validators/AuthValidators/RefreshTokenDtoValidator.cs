using FluentValidation;
using StockFlow.Application.DTOs.AuthDtos;

namespace StockFlow.Application.Validators.AuthValidators
{
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token boş bırakılamaz.")
                .MaximumLength(200).WithMessage("Refresh token 200 karakterden uzun olamaz.");
        }
    }
}