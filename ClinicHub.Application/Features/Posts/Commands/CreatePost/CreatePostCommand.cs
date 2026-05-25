using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Posts.Commands.CreatePost
{
    public record CreatePostCommand(string Content, IList<CreatePostMedia>? Media) : IRequest<string>;
    public record CreatePostMedia(string Url, MediaType Type);

    public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
    {
        public CreatePostCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(2000).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
            RuleFor(x => x.Media)
                .NotEmpty()
                .When(x => x.Media != null)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
            RuleForEach(x => x.Media).ChildRules(media =>
            {
                media.RuleFor(m => m.Url).NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
                media.RuleFor(m => m.Type).IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));
            });
        }
    }
}
