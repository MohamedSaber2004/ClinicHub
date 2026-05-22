using ClinicHub.Application.Features.Comments.DTOs;
using ClinicHub.Application.Common.Models;
using MediatR;
using FluentValidation;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using ClinicHub.Domain.Entities;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Comments.Queries.GetCommentReplies
{
    public record GetCommentRepliesQuery(Guid CommentId, 
        int PageNumber = PagginatedResult<CommentDto>.DefaultPageNumber, 
        int PageSize = PagginatedResult<CommentDto>.DefaultPageSize) : IRequest<PagginatedResult<CommentDto>>;

    public class GetCommentRepliesQueryValidator : AbstractValidator<GetCommentRepliesQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetCommentRepliesQueryValidator(IStringLocalizer<Messages> localizer,IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.CommentId).NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(CommentExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.CommentMessages.NotFound.Value]));
        }

        private Task<bool> CommentExists(Guid commentId, CancellationToken ct)
        {
            return _ctx.GetRepository<Comment, Guid>()
                .ExistsAsync(c => c.Id == commentId, ct);
        }
    }
}
