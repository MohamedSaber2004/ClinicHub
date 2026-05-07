using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
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
            var commentExists = await _unitOfWork.CommentRepository.ExistsAsync(
                c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);
            if (!commentExists)
                throw new NotFoundException($"Comment with id {request.CommentId} not found");

            var existingReaction = await _unitOfWork.ReactionRepository.GetFirstAsync(
                r => r.CommentId == request.CommentId &&
                     r.AuthorId == _currentUserService.UserId &&
                     !r.IsDeleted,
                cancellationToken);

            if (existingReaction != null)
            {
                if (existingReaction.Type == request.Type)
                {
                    _unitOfWork.ReactionRepository.Delete(existingReaction);
                }
                else
                {
                    existingReaction.UpdateReactionType(request.Type);
                    _unitOfWork.ReactionRepository.Update(existingReaction);
                }
            }
            else
            {
                var newReaction = new Reaction(request.Type, _currentUserService.UserId, commentId: request.CommentId);
                await _unitOfWork.ReactionRepository.AddAsync(newReaction);
            }

            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ?
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value) :
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
