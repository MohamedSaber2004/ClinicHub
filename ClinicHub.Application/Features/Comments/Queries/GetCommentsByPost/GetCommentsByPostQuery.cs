using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Comments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Comments.Queries.GetCommentsByPost
{
    public record GetCommentsByPostQuery(Guid PostId, 
        int PageNumber = PagginatedResult<CommentDto>.DefaultPageNumber, 
        int PageSize = PagginatedResult<CommentDto>.DefaultPageSize) : IRequest<PagginatedResult<CommentDto>>;

    public class GetCommentsByPostQueryValidator : AbstractValidator<GetCommentsByPostQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetCommentsByPostQueryValidator(IStringLocalizer<Messages> localizer,IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.PostId).NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(x => x.PostId)
                .MustAsync(PostExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PostMessages.NotFound.Value]));
        }

        private async Task<bool> PostExists(Guid postId, CancellationToken cancellationToken)
        {
            return await _ctx.GetRepository<Post, Guid>().ExistsAsync(p => p.Id == postId, cancellationToken);
        }
    }
}
