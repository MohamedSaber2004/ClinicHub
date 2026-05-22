using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.UpdateConversation
{
    public class UpdateConversationCommandHandler : IRequestHandler<UpdateConversationCommand, ConversationDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateConversationCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<ConversationDto> Handle(UpdateConversationCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            var conversation = await _unitOfWork.ConversationRepository.GetByIdAsync(request.ConversationId);

            // Only group conversations can be updated
            if (!conversation.IsGroup)
                throw new BadRequestException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ValidationMessages.InvalidOperation.Key]));

            // Authorization: Check if current user is the creator or a participant
            var isCreator = conversation.CreatedByUserId == currentUserId;
            var isParticipant = conversation.Participants.Any(p => p.UserId == currentUserId && !p.IsDeleted);

            if (!isCreator && !isParticipant)
                throw new BadRequestException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ValidationMessages.UnauthorizedAction.Key]));

            // Update conversation info with provided values, keeping existing values if not provided
            var updatedName = string.IsNullOrWhiteSpace(request.Name) ? conversation.Name : request.Name;
            conversation.UpdateGroupInfo(
                updatedName ?? "",
                request.GroupPhotoUrl ?? conversation.GroupPhotoUrl);

            _unitOfWork.ConversationRepository.Update(conversation);
            await _unitOfWork.SaveChangesAsync();

            // Map to DTO
            var conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                Name = conversation.Name,
                GroupPhotoUrl = conversation.GroupPhotoUrl,
                IsGroup = conversation.IsGroup,
                InitiatorId = conversation.InitiatorId,
                RecipientId = conversation.RecipientId,
                LastMessageDate = conversation.LastMessageDate,
                LastMessageContent = conversation.LastMessageContent,
                CreatedAt = conversation.CreatedAt,
                UnreadMessageCount = conversation.Messages.Count(m => m.SenderId != currentUserId && m.Status != ClinicHub.Domain.Enums.MessageStatus.Read)
            };

            return conversationDto;
        }
    }
}
