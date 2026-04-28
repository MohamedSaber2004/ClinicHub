using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Commands.AddComment
{
    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AddCommentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            Comment newComment;
            if (request.ParentCommentId.HasValue)
            {
                var parentComment = (await _unitOfWork.CommentRepository.GetByIdWithRepliesAsync(request.ParentCommentId.Value, cancellationToken))!;

                newComment = parentComment.AddReply(request.Content, _currentUserService.UserId);
                _unitOfWork.CommentRepository.Update(parentComment);
            }
            else
            {
                var post = (await _unitOfWork.PostRepository.GetByIdAsync(request.PostId))!;
                    
                newComment = post.AddComment(request.Content, _currentUserService.UserId);
                _unitOfWork.PostRepository.Update(post);
            }

            var result =await _unitOfWork.SaveChangesAsync();

            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value):
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
