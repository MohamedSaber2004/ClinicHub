using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SendMessageCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Guid> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            var conversation = await _unitOfWork.ConversationRepository.GetByIdWithMessagesAsync(request.ConversationId, cancellationToken);
            if (conversation == null)
                throw new NotFoundException(_localizer[LocalizationKeys.ValidationMessages.ConversationNotFound.Key]);

            if (conversation.InitiatorId != currentUserId && conversation.RecipientId != currentUserId)
                throw new BadRequestException(_localizer[LocalizationKeys.ValidationMessages.UnauthorizedAction.Key]);

            var message = new Message(request.ConversationId, currentUserId, request.Content);
            conversation.AddMessage(message);

            await _unitOfWork.MessageRepository.AddAsync(message);
            _unitOfWork.ConversationRepository.Update(conversation);
            await _unitOfWork.SaveChangesAsync();

            return message.Id;
        }
    }
}
