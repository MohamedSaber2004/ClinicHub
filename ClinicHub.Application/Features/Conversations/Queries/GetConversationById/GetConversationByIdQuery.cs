using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Conversations.Queries.GetConversationById
{
    public record GetConversationByIdQuery(Guid ConversationId) : IRequest<ConversationDetailDto>;

    public class GetConversationByIdQueryValidator : AbstractValidator<GetConversationByIdQuery>
    {
        public GetConversationByIdQueryValidator()
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));
        }
    }
}
