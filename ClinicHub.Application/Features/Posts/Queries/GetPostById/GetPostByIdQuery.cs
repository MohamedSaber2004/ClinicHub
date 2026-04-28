using ClinicHub.Application.Localization;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Posts.Queries.GetPostById
{
    public record GetPostByIdQuery(Guid PostId) : IRequest<PostDto>;

    public class GetPostByIdQueryValidator : AbstractValidator<GetPostByIdQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetPostByIdQueryValidator(IUnitOfWork ctx, IStringLocalizer<Messages> localizer)
        {
            _ctx = ctx;

            RuleFor(x => x.PostId).NotEmpty()
                .WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key]);

            RuleFor(x => x.PostId)
                .MustAsync(PostExists)
                .WithMessage(localizer[LocalizationKeys.PostMessages.NotFound.Key]);
        }

        private async Task<bool> PostExists(Guid postId, CancellationToken cancellationToken)
        {
            return await _ctx.GetRepository<Post, Guid>().ExistsAsync(p => p.Id == postId, cancellationToken);
        }
    }
}
