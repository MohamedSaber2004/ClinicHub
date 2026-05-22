using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.SendMessage
{
    public record SendMessageCommand(
        Guid ConversationId, 
        string? Content, 
        Guid? ReplyToMessageId = null,
        List<MessageMediaInputDto>? Media = null) : IRequest<MessageDto>;

    public class MessageMediaInputDto
    {
        public MediaType MediaType { get; set; }
        public string FileName { get; set; } = string.Empty;
    }

    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .When(x => x.Media == null || !x.Media.Any());

            RuleFor(x => x.Content)
                .MaximumLength(5000)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
        }
    }
}
