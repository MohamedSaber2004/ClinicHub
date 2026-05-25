using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Queries.GetConversationMessages
{
    public record GetConversationMessagesQuery(Guid ConversationId, int PageNumber = 1, int PageSize = 50) : IRequest<PagginatedResult<MessageDto>>;

    public class GetConversationMessagesQueryValidator : AbstractValidator<GetConversationMessagesQuery>
    {
        public GetConversationMessagesQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageNumberMustBeGreaterThanOrEqualToOne.Value]));

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeGreaterThanOrEqualToOne.Value]))
                .LessThanOrEqualTo(100)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeLessThanOrEqualToHundred.Value]));
        }
    }
}
