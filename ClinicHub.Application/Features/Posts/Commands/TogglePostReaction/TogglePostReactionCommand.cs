using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Commands.TogglePostReaction
{
    public record TogglePostReactionCommand(Guid PostId, ReactionType Type) : IRequest<string>;

    public class TogglePostReactionCommandValidator : AbstractValidator<TogglePostReactionCommand>
    {
        private readonly IUnitOfWork _ctx;

        public TogglePostReactionCommandValidator(IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.PostId).NotEmpty()
               .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value)); 
               
            RuleFor(x => x.Type).IsInEnum()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidEnumValue.Value));

            RuleFor(x => x.PostId)
                .MustAsync(PostExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.PostMessages.NotFound.Value));
        }

        private async Task<bool> PostExists(Guid postId, CancellationToken cancellationToken)
        {
            return await _ctx.GetRepository<Post,Guid>().ExistsAsync(p => p.Id == postId, cancellationToken);
        }
    }
}
