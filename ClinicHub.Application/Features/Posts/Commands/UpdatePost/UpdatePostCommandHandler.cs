using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Commands.UpdatePost
{
    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdatePostCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<Post, Guid>();
            var post = (await repo.GetByIdAsync(request.PostId))!;

            post.UpdateContent(request.Content);
            
            repo.Update(post);
            var result = await _unitOfWork.SaveChangesAsync();
            
            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value):
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
