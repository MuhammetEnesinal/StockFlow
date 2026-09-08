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
        }
    }
}