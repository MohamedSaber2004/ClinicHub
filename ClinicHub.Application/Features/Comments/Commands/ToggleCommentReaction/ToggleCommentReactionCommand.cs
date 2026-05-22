using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Comments.Commands.ToggleCommentReaction
{
    public record ToggleCommentReactionCommand(Guid CommentId, ReactionType Type) : IRequest<string>;

    public class ToggleCommentReactionCommandValidator : AbstractValidator<ToggleCommentReactionCommand>
    {
        private readonly IUnitOfWork _ctx;

        public ToggleCommentReactionCommandValidator(IStringLocalizer<Messages> localizer,IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.CommentId).NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(CommentExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.CommentMessages.NotFound.Value]));
            
            RuleFor(x => x.Type).IsInEnum()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));
        }

        private async Task<bool> CommentExists(Guid commentId, CancellationToken ct)
        {
            return await _ctx.GetRepository<Comment, Guid>().ExistsAsync(c => c.Id == commentId, ct);
        }
    }
}
