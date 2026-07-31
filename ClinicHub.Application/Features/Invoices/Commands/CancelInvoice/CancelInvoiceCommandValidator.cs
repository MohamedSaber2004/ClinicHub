using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Commands.CancelInvoice;

public class CancelInvoiceCommandValidator : AbstractValidator<CancelInvoiceCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelInvoiceCommandValidator(
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
                return invoice == null || invoice.Status != InvoiceStatus.Cancelled;
            }).WithMessage(localizer[LocalizationKeys.InvoiceMessages.AlreadyCancelled.Value]);

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value, localizer["Reason"]])
            .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);
    }
}
