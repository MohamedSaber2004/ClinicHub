using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Commands.AddComment
{
    public record AddCommentCommand(Guid PostId, string Content, Guid? ParentCommentId) : IRequest<string>;

    public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
    {
        private readonly IUnitOfWork _ctx;

        public AddCommentCommandValidator(IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.PostId).NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MaximumLength(2000).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value));
        
            RuleFor(x => x.PostId)
                .MustAsync(PostExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.PostMessages.NotFound.Value));
                
            RuleFor(x => x.ParentCommentId)
                .MustAsync(CommentExists)
                .When(x => x.ParentCommentId.HasValue)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.CommentMessages.NotFound.Value));
        }

        private async Task<bool> PostExists(Guid postId, CancellationToken ct)
        {
            return await _ctx.GetRepository<Post, Guid>().ExistsAsync(p => p.Id == postId, ct);
        }

        private async Task<bool> CommentExists(Guid? commentId, CancellationToken ct)
        {
            if (!commentId.HasValue) return true;
            return await _ctx.GetRepository<Comment, Guid>().ExistsAsync(c => c.Id == commentId.Value, ct);
        }
    }
}
