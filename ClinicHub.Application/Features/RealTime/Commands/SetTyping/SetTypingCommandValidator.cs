using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Commands.SetTyping
{
    public class SetTypingCommandValidator : AbstractValidator<SetTypingCommand>
    {
        private readonly IUnitOfWork _ctx;

        public SetTypingCommandValidator(IStringLocalizer<Message> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;


            RuleFor(x => x.ConversationId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RealTimeMessages.ConversationIdRequired.Value]))
                .MustAsync(ConversationFound).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RealTimeMessages.ConversationNotFound.Value]));
        }

        private async Task<bool> ConversationFound(Guid conversationId, CancellationToken cancellationToken)
        {
            return await _ctx.ConversationRepository.ExistsAsync(c => c.Id == conversationId, cancellationToken);
        }
    }
}
