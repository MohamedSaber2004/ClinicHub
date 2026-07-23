using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Advertisements.Commands.DeleteAdvertisement
{
    public class DeleteAdvertisementCommandValidator : AbstractValidator<DeleteAdvertisementCommand>
    {
        private readonly IUnitOfWork _ctx;

        public DeleteAdvertisementCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.Id)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (id, ct) =>
                    await _ctx.GetRepository<Advertisement, Guid>().ExistsAsync(a => a.Id == id, ct))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AdvertisementMessages.NotFound.Value]));
        }
    }
}
