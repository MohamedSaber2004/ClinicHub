using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Queries.GetInvoicesByClinic;

public class GetInvoicesByClinicQueryValidator : AbstractValidator<GetInvoicesByClinicQuery>
{
    public GetInvoicesByClinicQueryValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage(localizer[LocalizationKeys.ValidationMessages.PageNumberMustBeGreaterThanOrEqualToOne.Value]);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeGreaterThanOrEqualToOne.Value])
            .LessThanOrEqualTo(100).WithMessage(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeLessThanOrEqualToHundred.Value]);

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]);

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate).When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidDateRange.Value]);
    }
}
