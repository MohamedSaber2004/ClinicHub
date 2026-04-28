using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Posts.Commands.UpdatePost
{
    public record UpdatePostCommand(Guid PostId, string Content) : IRequest<string>;

    public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
    {
        private readonly IUnitOfWork _ctx;
        private readonly ICurrentUserService _currentUserService;

        public UpdatePostCommandValidator(IUnitOfWork ctx, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _ctx = ctx;
            _currentUserService = currentUserService;

            RuleFor(x => x.PostId).NotEmpty();
            
            RuleFor(x => x.PostId)
                .CustomAsync(async (postId, context, cancellationToken) =>
                {
                    if (!await IsAuthor(postId, cancellationToken))
                    {
                        context.AddFailure(localizer[LocalizationKeys.ExceptionMessages.Unauthorized.Key]);
                    }
                })
                .MustAsync(PostExists)
                .WithMessage(localizer[LocalizationKeys.PostMessages.NotFound.Key]);

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key])
                .MaximumLength(2000).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Key]);
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
