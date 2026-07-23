using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Subscriptions.Commands.InitiateSubscriptionPayment
{
    public class InitiateSubscriptionPaymentCommandValidator : AbstractValidator<InitiateSubscriptionPaymentCommand>
    {
        private readonly IUnitOfWork _ctx;

        public InitiateSubscriptionPaymentCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.PlanId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (id, ct) =>
                    await _ctx.GetRepository<Plan, Guid>().ExistsAsync(p => p.Id == id && !p.IsDeleted && p.IsActive, ct))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PlanMessages.NotFound.Value]));

            RuleFor(v => v.Period)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));
        }
    }
}
