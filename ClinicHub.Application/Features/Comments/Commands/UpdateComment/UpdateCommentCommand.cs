using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Commands.UpdateComment
{
    public record UpdateCommentCommand(Guid CommentId, string Content) : IRequest<string>;

    public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
    {
        private readonly IUnitOfWork _ctx;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCommentCommandValidator(IUnitOfWork ctx, ClinicHub.Application.Common.Interfaces.ICurrentUserService currentUserService)
        {
            _ctx = ctx;
            _currentUserService = currentUserService;

            RuleFor(x => x.CommentId).NotEmpty();
            
            RuleFor(x => x.CommentId)
                .CustomAsync(async (commentId, context, cancellationToken) =>
                {
                    if (!await IsAuthor(commentId, cancellationToken))
                    {
                        context.AddFailure(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ExceptionMessages.Unauthorized.Value));
                    }
                })
                .MustAsync(CommentExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.CommentMessages.NotFound.Value));

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MaximumLength(2000).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value));
        }

        private async Task<bool> CommentExists(Guid commentId, CancellationToken cancellationToken)
        {
            return await _ctx.GetRepository<Comment,Guid>().ExistsAsync(c => c.Id == commentId, cancellationToken);
        }

        private async Task<bool> IsAuthor(Guid commentId, CancellationToken cancellationToken)
        {
            var comment = await _ctx.GetRepository<Comment, Guid>().GetByIdAsync(commentId);
            var currentUserId = _currentUserService.UserId;
            return comment != null && comment.AuthorId == currentUserId;
        }
    }
}
