using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Commands.TogglePostReaction
{
    public class TogglePostReactionCommandHandler : IRequestHandler<TogglePostReactionCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public TogglePostReactionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(TogglePostReactionCommand request, CancellationToken cancellationToken)
        {
            var post = (await _unitOfWork.PostRepository.GetByIdWithDetailsAsync(request.PostId, cancellationToken))!;

            post.AddReaction(request.Type, _currentUserService.UserId);

            _unitOfWork.PostRepository.Update(post);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value):
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
