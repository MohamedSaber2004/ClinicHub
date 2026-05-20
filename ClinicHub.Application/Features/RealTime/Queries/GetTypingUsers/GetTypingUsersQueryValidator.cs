using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Queries.GetTypingUsers
{
    public class GetTypingUsersQueryValidator : AbstractValidator<GetTypingUsersQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetTypingUsersQueryValidator(IStringLocalizer<Message> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.ConversationId)
                .NotEmpty().WithMessage(localizer["RealTime:ConversationIdRequired"])
                .MustAsync(ConversationFound).WithMessage(localizer["RealTime:ConversationNotFound"]);
        }

        private async Task<bool> ConversationFound(Guid conversationId, CancellationToken cancellationToken)
        {
            return await _ctx.ConversationRepository.ExistsAsync(c => c.Id == conversationId, cancellationToken);
        }
    }
}
