using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Commands.CreateDraftInvoice;

public class CreateDraftInvoiceCommandValidator : AbstractValidator<CreateDraftInvoiceCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateDraftInvoiceCommandValidator(
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<Messages> localizer)
    {
        _userManager = userManager;

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value, localizer["Items"]]);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value, localizer["Description"]])
                .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

            item.RuleFor(i => i.Discount)
                .GreaterThanOrEqualTo(0).When(i => i.Discount.HasValue)
                .WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value])
                .LessThanOrEqualTo(100).When(i => i.Discount.HasValue)
                .WithMessage(localizer[LocalizationKeys.InvoiceMessages.DiscountOutOfRange.Value]);
        });

        RuleFor(x => x.DiscountType)
            .IsInEnum().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]);

        RuleFor(x => x.DiscountValue)
            .GreaterThanOrEqualTo(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value])
            .LessThanOrEqualTo(100).When(x => x.DiscountType == DiscountType.Percentage)
            .WithMessage(localizer[LocalizationKeys.InvoiceMessages.DiscountOutOfRange.Value]);

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100).WithMessage(localizer[LocalizationKeys.InvoiceMessages.TaxOutOfRange.Value]);

        RuleFor(x => x.PatientId)
            .MustAsync(async (id, ct) =>
            {
                if (!id.HasValue) return true;
                var user = await _userManager.FindByIdAsync(id.Value.ToString());
                return user != null;
            }).WithMessage(localizer[LocalizationKeys.ValidationMessages.UserNotFound.Value]);
    }
}
