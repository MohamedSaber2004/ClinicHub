using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Commands.RecordPayment;

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RecordPaymentCommandValidator(
        IUnitOfWork unitOfWork,
        IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value, localizer["Invoice"]])
            .MustAsync(async (id, ct) => await _unitOfWork.InvoiceRepository.ExistsByKeyAsync(id, ct))
                .WithMessage(localizer[LocalizationKeys.InvoiceMessages.NotFound.Value])
            .MustAsync(async (id, ct) =>
            {
                var invoice = await _unitOfWork.InvoiceRepository.GetByIdAsync(id);
                return invoice == null || invoice.Status == InvoiceStatus.Issued;
            }).WithMessage(localizer[LocalizationKeys.InvoiceMessages.CannotPayNotIssued.Value]);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value])
            .MustAsync(async (cmd, amount, ct) =>
            {
                var invoice = await _unitOfWork.InvoiceRepository.GetByIdAsync(cmd.InvoiceId);
                return invoice == null || amount <= invoice.Total;
            }).WithMessage(localizer[LocalizationKeys.InvoiceMessages.AmountExceedsTotal.Value]);

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]);

        RuleFor(x => x.TransactionRef)
            .NotEmpty().When(x => x.Method == PaymentMethodType.Card || x.Method == PaymentMethodType.Wallet)
            .WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value, localizer["TransactionRef"]])
            .MaximumLength(100).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);
    }
}
