using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Commands.CreatePost
{
    public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreatePostCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        {
            var post = new Post(request.Content, _currentUserService.UserId);

            if (request.Media != null)
            {
                foreach (var media in request.Media)
                {
                    post.AddMedia(media.Url, media.Type);
                }
            }

            var repo = _unitOfWork.GetRepository<Post, Guid>();
            await repo.AddAsync(post);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value):
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
