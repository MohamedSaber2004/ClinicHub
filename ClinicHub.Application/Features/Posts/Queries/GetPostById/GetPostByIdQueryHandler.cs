using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Posts.Queries.GetPostById
{
    public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetPostByIdQueryHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<PostDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
        {
            var post = (await _unitOfWork.PostRepository.GetByIdWithDetailsAsync(request.PostId, cancellationToken))!;
            var author = await _unitOfWork.GetRepository<ApplicationUser, Guid>().GetByIdAsync(post.AuthorId);
            var doctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.UserId == post.AuthorId)
                .FirstOrDefaultAsync(cancellationToken);
            var roles = author != null ? await _userManager.GetRolesAsync(author) : Array.Empty<string>();

            return new PostDto(
                post.Id,
                post.Content,
                post.AuthorId,
                author?.FullName ?? "Unknown",
                author?.ProfilePictureUrl ?? string.Empty,
                post.CreatedAt,
                post.Reactions.Count,
                post.Comments.Count,
                doctor != null && doctor.IsFreelance,
                post.Media.Select(m => new MediaDto(m.Id, m.Url, m.Type.ToString())).ToList(),
                roles.FirstOrDefault()
            );
        }
    }
}
