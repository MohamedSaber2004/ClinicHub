using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Queries.GetPostById
{
    public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPostByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PostDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
        {
            var post = (await _unitOfWork.PostRepository.GetByIdWithDetailsAsync(request.PostId, cancellationToken))!;

            return new PostDto(
                post.Id,
                post.Content,
                post.AuthorId,
                post.CreatedAt,
                post.Reactions.Count,
                post.Comments.Count,
                post.Media.Select(m => new MediaDto(m.Id, m.Url, m.Type.ToString())).ToList()
            );
        }
    }
}
