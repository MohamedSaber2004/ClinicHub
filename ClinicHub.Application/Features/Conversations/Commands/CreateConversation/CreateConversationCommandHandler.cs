using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.CreateConversation
{
    public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public CreateConversationCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Guid> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            if (currentUserId == request.RecipientId)
                throw new BadRequestException(_localizer[LocalizationKeys.ValidationMessages.CannotChatWithYourself.Key]);

            var existingConversation = await _unitOfWork.ConversationRepository.GetConversationBetweenUsersAsync(currentUserId, request.RecipientId, cancellationToken);
            if (existingConversation != null)
                return existingConversation.Id;

            var conversation = new Conversation(currentUserId, request.RecipientId);
            await _unitOfWork.ConversationRepository.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();

            return conversation.Id;
        }
    }
}
