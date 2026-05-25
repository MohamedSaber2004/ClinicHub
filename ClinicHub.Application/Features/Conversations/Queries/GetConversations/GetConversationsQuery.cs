using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Common.Models;
using FluentValidation;
using MediatR;
using ClinicHub.Application.Localization;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Queries.GetConversations
{
    public record GetConversationsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagginatedResult<ConversationDto>>;

    public class GetConversationsQueryValidator : AbstractValidator<GetConversationsQuery>
    {
        public GetConversationsQueryValidator(IStringLocalizer<Messages> localizer)
        {
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
