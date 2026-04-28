using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Commands.DeleteComment
{
    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCommentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = (await _unitOfWork.CommentRepository.GetByIdAsync(request.CommentId))!;

            comment.MarkAsDeleted(_currentUserService.UserId.ToString());
            
            _unitOfWork.CommentRepository.Update(comment);
            var result = await _unitOfWork.SaveChangesAsync();
            
            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value):
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
