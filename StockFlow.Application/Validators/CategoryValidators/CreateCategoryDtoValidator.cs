using FluentValidation;
using StockFlow.Application.DTOs.CategoryDtos;


namespace StockFlow.Application.Validators.CategoryValidators
{
    public class CreateCategoryDtoValidator:AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator() { 
            RuleFor(x=> x.Name).NotEmpty().WithMessage("Kategori Adı Boş Olamaz").MaximumLength(200).WithMessage("Kategori adı 200 karakterden uzun olamaz");
        
            
        }
    }
}
