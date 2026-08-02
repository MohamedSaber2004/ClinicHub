using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Commands.CreateClinicAdOrder;

public class CreateClinicAdOrderCommandValidator : AbstractValidator<CreateClinicAdOrderCommand>
{
    public CreateClinicAdOrderCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
    {
        RuleFor(v => v.ClinicId)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
            .MustAsync(async (id, ct) =>
                await ctx.ClinicRepository.ExistsAsync(c => c.Id == id && !c.IsDeleted, ct))
            .WithMessage(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]);

        RuleFor(v => v.AdPackageId)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
            .MustAsync(async (id, ct) =>
                await ctx.GetRepository<AdPackage, Guid>().ExistsAsync(p => p.Id == id && !p.IsDeleted, ct))
            .WithMessage(localizer[LocalizationKeys.PaymentMessages.AdPackageNotFound.Value]);

        RuleFor(v => v.DurationDays)
            .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

        RuleFor(v => v.ReturnUrl)
            .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);
    }
}
