using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Posts.Commands.CreatePost
{
    public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public CreatePostCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;
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
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Success.Value]) :
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Error.Value]);
        }
    }
}
