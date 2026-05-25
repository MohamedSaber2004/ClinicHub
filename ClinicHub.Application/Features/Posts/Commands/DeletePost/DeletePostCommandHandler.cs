using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Posts.Commands.DeletePost
{
    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeletePostCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<string> Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<Post, Guid>();
            var post = (await repo.GetByIdAsync(request.PostId))!;

            post.MarkAsDeleted(_currentUserService.UserId.ToString());
            
            repo.Update(post);
            var result = await _unitOfWork.SaveChangesAsync();
            
            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Success.Value]):
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Error.Value]);
        }
    }
}
