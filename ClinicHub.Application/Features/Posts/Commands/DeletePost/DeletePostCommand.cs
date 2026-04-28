using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Commands.DeletePost
{
    public record DeletePostCommand(Guid PostId) : IRequest<string>;

    public class DeletePostCommandValidator : AbstractValidator<DeletePostCommand>
    {
        private readonly IUnitOfWork _ctx;
        private readonly ICurrentUserService _currentUserService;

        public DeletePostCommandValidator(IUnitOfWork ctx, ICurrentUserService currentUserService)
        {
            _ctx = ctx;
            _currentUserService = currentUserService;

            RuleFor(x => x.PostId).NotEmpty();

            RuleFor(x => x.PostId)
                .CustomAsync(async (postId, context, cancellationToken) =>
                {
                    if (!await IsAuthor(postId, cancellationToken))
                    {
                        context.AddFailure(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ExceptionMessages.Unauthorized.Value));
                    }
                })
                .MustAsync(PostExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.PostMessages.NotFound.Value));
        }

        private async Task<bool> PostExists(Guid postId, CancellationToken cancellationToken)
        {
            return await _ctx.GetRepository<Post,Guid>().ExistsAsync(p => p.Id == postId, cancellationToken);
        }

        private async Task<bool> IsAuthor(Guid postId, CancellationToken cancellationToken)
        {
            var post = await _ctx.GetRepository<Post, Guid>().GetByIdAsync(postId);
            var currentUserId = _currentUserService.UserId;
            return post != null && post.AuthorId == currentUserId;
        }
    }
}
