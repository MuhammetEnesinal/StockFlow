using FluentValidation;
using StockFlow.Application.DTOs.CategoryDtos;

namespace StockFlow.Application.Validators.CategoryValidators
{
    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Kategori adı boş olamaz.")
                .MaximumLength(200).WithMessage("Kategori adı 200 karakterden uzun olamaz.");
        }
    }
}