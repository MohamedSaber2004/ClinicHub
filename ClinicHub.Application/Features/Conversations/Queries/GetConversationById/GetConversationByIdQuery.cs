using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Queries.GetConversationById
{
    public record GetConversationByIdQuery(Guid ConversationId) : IRequest<ConversationDetailDto>;

    public class GetConversationByIdQueryValidator : AbstractValidator<GetConversationByIdQuery>
    {
        public GetConversationByIdQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
        }
    }
}
