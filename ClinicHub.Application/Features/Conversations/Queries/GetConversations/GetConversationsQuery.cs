using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Conversations.Queries.GetConversations
{
    public record GetConversationsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagginatedResult<ConversationDto>>;

    public class GetConversationsQueryValidator : AbstractValidator<GetConversationsQuery>
    {
        public GetConversationsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be greater than or equal to 1");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page size must be greater than or equal to 1")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be less than or equal to 100");
        }
    }
}
