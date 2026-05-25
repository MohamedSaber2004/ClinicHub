using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Commands.SetActiveConversation
{
    public class SetActiveConversationCommandValidator: AbstractValidator<SetActiveConversationCommand>
    {
        private readonly IUnitOfWork _ctx;

        public SetActiveConversationCommandValidator(IStringLocalizer<Message> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.ConversationId)
                .MustAsync(ConversationFound).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RealTimeMessages.ConversationNotFound.Value]))
                .When(c => c.ConversationId is not null);
        }

        private async Task<bool> ConversationFound(Guid? conversationId, CancellationToken cancellationToken)
        {
            return await _ctx.ConversationRepository.ExistsAsync(c => c.Id == conversationId, cancellationToken);
        }
    }
}
