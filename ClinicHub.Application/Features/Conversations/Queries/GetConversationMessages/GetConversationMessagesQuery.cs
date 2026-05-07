using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Conversations.Queries.GetConversationMessages
{
    public record GetConversationMessagesQuery(Guid ConversationId, int PageNumber = 1, int PageSize = 50) : IRequest<PagginatedResult<MessageDto>>;

    public class GetConversationMessagesQueryValidator : AbstractValidator<GetConversationMessagesQuery>
    {
        public GetConversationMessagesQueryValidator()
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be greater than or equal to 1");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page size must be greater than or equal to 1")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be less than or equal to 100");
        }
    }
}
