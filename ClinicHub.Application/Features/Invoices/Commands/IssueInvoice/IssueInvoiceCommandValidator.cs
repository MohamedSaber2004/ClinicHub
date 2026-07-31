using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Commands.IssueInvoice;

public class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public IssueInvoiceCommandValidator(
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
                return invoice == null || invoice.Status == InvoiceStatus.Draft;
            }).WithMessage(localizer[LocalizationKeys.InvoiceMessages.AlreadyIssued.Value]);
    }
}
