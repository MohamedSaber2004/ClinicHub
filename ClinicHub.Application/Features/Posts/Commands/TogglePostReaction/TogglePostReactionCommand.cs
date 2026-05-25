using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Posts.Commands.TogglePostReaction
{
    public record TogglePostReactionCommand(Guid PostId, ReactionType Type) : IRequest<string>;

    public class TogglePostReactionCommandValidator : AbstractValidator<TogglePostReactionCommand>
    {
        private readonly IUnitOfWork _ctx;

        public TogglePostReactionCommandValidator(IUnitOfWork ctx, IStringLocalizer<Messages> localizer)
        {
            _ctx = ctx;

            RuleFor(x => x.PostId).NotEmpty()
               .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(x => x.Type).IsInEnum()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));

            RuleFor(x => x.PostId)
                .MustAsync(PostExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PostMessages.NotFound.Value]));
        }

        private async Task<bool> PostExists(Guid postId, CancellationToken cancellationToken)
        {
            return await _ctx.GetRepository<Post, Guid>().ExistsAsync(p => p.Id == postId, cancellationToken);
        }
    }
}
