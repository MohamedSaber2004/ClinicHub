using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Commands.UpdateComment
{
    public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCommentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<Comment, Guid>();
            var comment = (await repo.GetByIdAsync(request.CommentId))!;

            comment.UpdateContent(request.Content);
            
            repo.Update(comment);
            var result = await _unitOfWork.SaveChangesAsync();
            
            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value):
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
