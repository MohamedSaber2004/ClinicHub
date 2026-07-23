using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Admin.Commands.UpdateSupportTicketStatus
{
    public class UpdateSupportTicketStatusCommandValidator : AbstractValidator<UpdateSupportTicketStatusCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateSupportTicketStatusCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.TicketId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (id, ct) =>
                    await _ctx.GetRepository<SupportTicket, Guid>().ExistsAsync(t => t.Id == id, ct))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.SupportTicketMessages.NotFound.Value]));

            RuleFor(v => v.Status)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));
        }
    }
}
