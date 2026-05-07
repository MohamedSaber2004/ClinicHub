using ClinicHub.Application.Common.Exceptions;
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
                var postExists = await _unitOfWork.PostRepository.ExistsAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);
                if (!postExists)
                    throw new NotFoundException($"Post with id {request.PostId} not found");

                newComment = new Comment(request.Content, _currentUserService.UserId, request.PostId);
                await _unitOfWork.CommentRepository.AddAsync(newComment);
            }

            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ?
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value) :
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
