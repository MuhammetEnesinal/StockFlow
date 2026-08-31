using FluentValidation;
using StockFlow.Application.DTOs.ProductDtos;

namespace StockFlow.Application.Validators.ProductValidators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ürün adı boş olamaz.")
                .MaximumLength(200).WithMessage("Ürün adı 200 karakterden uzun olamaz.");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU boş olamaz.")
                .MaximumLength(50).WithMessage("SKU 50 karakterden uzun olamaz.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.")
                .Must(price => decimal.Round(price, 2) == price)
                .WithMessage("Fiyat en fazla 2 ondalık basamak içerebilir.");

            RuleFor(x => x.MinimumStockLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stok seviyesi negatif olamaz.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Geçerli bir kategori seçilmelidir.");

            RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("Geçerli bir tedarikçi seçilmelidir.")
                .When(x => x.SupplierId.HasValue);
        }
    }
}