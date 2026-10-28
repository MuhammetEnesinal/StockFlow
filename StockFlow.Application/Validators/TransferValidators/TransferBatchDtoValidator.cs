using FluentValidation;
using StockFlow.Application.DTOs.TransferDtos;

namespace StockFlow.Application.Validators.TransferValidators
{
    public class TransferBatchDtoValidator : AbstractValidator<TransferBatchDto>
    {
        public TransferBatchDtoValidator()
        {
            RuleFor(x => x.Transfers)
                .NotEmpty().WithMessage("En az bir transfer işlemi gerekli.");

            RuleFor(x => x.Transfers)
                .Must(items => items.Count <= 100).WithMessage("Toplu transferde en fazla 100 işlem olabilir.")
                .When(x => x.Transfers != null);
        }
    }
}