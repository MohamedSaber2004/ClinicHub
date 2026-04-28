using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Commands.ToggleCommentReaction
{
    public class ToggleCommentReactionCommandHandler : IRequestHandler<ToggleCommentReactionCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ToggleCommentReactionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(ToggleCommentReactionCommand request, CancellationToken cancellationToken)
        {
            var comment = (await _unitOfWork.CommentRepository.GetByIdWithRepliesAsync(request.CommentId, cancellationToken))!;

            comment.ToggleReaction(request.Type, _currentUserService.UserId);

            _unitOfWork.CommentRepository.Update(comment);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value):
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
