using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Subscriptions.Commands.RevokeSubscription
{
    public class RevokeSubscriptionCommandValidator : AbstractValidator<RevokeSubscriptionCommand>
    {
        private readonly IUnitOfWork _ctx;

        public RevokeSubscriptionCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.SubscriptionId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (id, ct) =>
                    await _ctx.GetRepository<Subscription, Guid>().ExistsAsync(s => s.Id == id, ct))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.SubscriptionMessages.NotFound.Value]));
        }
    }
}
