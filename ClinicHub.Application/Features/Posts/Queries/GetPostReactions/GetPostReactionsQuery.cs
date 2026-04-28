using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Queries.GetPostReactions
{
    public record GetPostReactionsQuery(Guid PostId, 
        int PageNumber = PagginatedResult<ReactionDto>.DefaultPageNumber, 
        int PageSize = PagginatedResult<ReactionDto>.DefaultPageSize) : IRequest<PagginatedResult<ReactionDto>>;

    public class GetPostReactionsQueryValidator : AbstractValidator<GetPostReactionsQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetPostReactionsQueryValidator(IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.PostId).NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));

            RuleFor(x => x.PostId)
                .MustAsync(PostExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.PostMessages.NotFound.Value));
        }

        private async Task<bool> PostExists(Guid postId, CancellationToken ct)
        {
            return await _ctx.GetRepository<Post, Guid>().ExistsAsync(p => p.Id == postId, ct);
        }
    }
}
