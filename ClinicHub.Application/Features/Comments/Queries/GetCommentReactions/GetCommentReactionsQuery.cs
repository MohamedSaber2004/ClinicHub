using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Comments.Queries.GetCommentReactions
{
    public record GetCommentReactionsQuery(Guid CommentId, 
        int PageNumber = PagginatedResult<ReactionDto>.DefaultPageNumber, 
        int PageSize = PagginatedResult<ReactionDto>.DefaultPageSize) : IRequest<PagginatedResult<ReactionDto>>;

    public class GetCommentReactionsQueryValidator : AbstractValidator<GetCommentReactionsQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetCommentReactionsQueryValidator(IStringLocalizer<Messages> localizer,IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.CommentId).NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(x => x.CommentId)
                .MustAsync(CommentExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.CommentMessages.NotFound.Value]));
        }

        private async Task<bool> CommentExists(Guid commentId, CancellationToken ct)
        {
            return await _ctx.GetRepository<Comment, Guid>().ExistsAsync(c => c.Id == commentId, ct);
        }
    }
}
