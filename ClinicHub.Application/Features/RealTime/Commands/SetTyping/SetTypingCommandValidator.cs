using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Commands.SetTyping
{
    public class SetTypingCommandValidator : AbstractValidator<SetTypingCommand>
    {
        private readonly IUnitOfWork _ctx;

        public SetTypingCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.ConversationId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.RealTimeMessages.ConversationIdRequired])
                .MustAsync(ConversationFound).WithMessage(localizer[LocalizationKeys.RealTimeMessages.ConversationNotFound]);
        }

        private async Task<bool> ConversationFound(Guid conversationId, CancellationToken cancellationToken)
        {
            return await _ctx.ConversationRepository.ExistsAsync(c => c.Id == conversationId, cancellationToken);
        }
    }
}
