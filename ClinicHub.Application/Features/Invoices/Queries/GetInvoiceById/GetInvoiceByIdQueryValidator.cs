using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInvoiceByIdQueryValidator(
        IUnitOfWork unitOfWork,
        IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value, localizer["Invoice"]])
            .MustAsync(async (id, ct) => await _unitOfWork.InvoiceRepository.ExistsByKeyAsync(id, ct))
                .WithMessage(localizer[LocalizationKeys.InvoiceMessages.NotFound.Value]);
    }
}
