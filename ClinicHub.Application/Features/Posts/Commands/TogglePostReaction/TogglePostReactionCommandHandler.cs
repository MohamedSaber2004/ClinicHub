using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
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
            var postExists = await _unitOfWork.PostRepository.ExistsAsync(
                p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);
            if (!postExists)
                throw new NotFoundException($"Post with id {request.PostId} not found");

            var existingReaction = await _unitOfWork.ReactionRepository.GetFirstAsync(
                r => r.PostId == request.PostId &&
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
                var newReaction = new Reaction(request.Type, _currentUserService.UserId, postId: request.PostId);
                await _unitOfWork.ReactionRepository.AddAsync(newReaction);
            }

            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ?
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value) :
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
