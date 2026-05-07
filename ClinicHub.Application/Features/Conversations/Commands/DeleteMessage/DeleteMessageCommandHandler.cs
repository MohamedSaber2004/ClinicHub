using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.DeleteMessage
{
    public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteMessageCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<string> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            var message = await _unitOfWork.MessageRepository.GetByIdAsync(request.MessageId);
            if (message == null)
                throw new NotFoundException(_localizer[LocalizationKeys.ValidationMessages.MessageNotFound.Key]);

            if (message.SenderId != currentUserId)
                throw new BadRequestException(_localizer[LocalizationKeys.ValidationMessages.UnauthorizedAction.Key]);

            message.MarkAsDeleted(currentUserId.ToString());
            _unitOfWork.MessageRepository.Update(message);
            await _unitOfWork.SaveChangesAsync();

            return _localizer[LocalizationKeys.ValidationMessages.DeletedSuccessfully.Key];
        }
    }
}
