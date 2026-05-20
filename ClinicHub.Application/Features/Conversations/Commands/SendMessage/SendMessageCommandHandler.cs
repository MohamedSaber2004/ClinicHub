using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;
using AutoMapper;
using ClinicHub.Application.Features.Conversations.DTOs;

namespace ClinicHub.Application.Features.Conversations.Commands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        private readonly IPusherService _pusherService;
        private readonly IMapper _mapper;
        private readonly IChatConnectionManager _chatConnectionManager;

        public SendMessageCommandHandler(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUserService, 
            IStringLocalizer<Messages> localizer, 
            IPusherService pusherService, 
            IMapper mapper,
            IChatConnectionManager chatConnectionManager)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _pusherService = pusherService;
            _mapper = mapper;
            _chatConnectionManager = chatConnectionManager;
        }

        public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            var conversation = await _unitOfWork.ConversationRepository.GetByIdWithMessagesAsync(request.ConversationId, cancellationToken);
            if (conversation == null)
                throw new NotFoundException(_localizer[LocalizationKeys.ValidationMessages.ConversationNotFound.Key]);

            if (conversation.InitiatorId != currentUserId && conversation.RecipientId != currentUserId)
                throw new BadRequestException(_localizer[LocalizationKeys.ValidationMessages.UnauthorizedAction.Key]);

            // Validate ReplyToMessageId if provided
            Message? repliedMessage = null;
            if (request.ReplyToMessageId.HasValue)
            {
                repliedMessage = await _unitOfWork.MessageRepository.GetByIdAsync(request.ReplyToMessageId.Value);
                if (repliedMessage == null || repliedMessage.ConversationId != request.ConversationId || repliedMessage.IsDeleted)
                {
                    throw new NotFoundException(_localizer[LocalizationKeys.ValidationMessages.MessageNotFound.Key]);
                }
            }

            var recipientId = conversation.InitiatorId == currentUserId ? conversation.RecipientId : conversation.InitiatorId;

            var message = new Message(request.ConversationId, currentUserId, request.Content, request.ReplyToMessageId);

            // Process Media Attachments if any
            if (request.Media != null && request.Media.Any())
            {
                foreach (var mediaInput in request.Media)
                {
                    var media = new MessageMedia(message.Id, mediaInput.MediaType, mediaInput.FileName);
                    message.AddMedia(media);
                }
            }

            if (_chatConnectionManager.IsUserInConversation(recipientId, request.ConversationId))
            {
                message.MarkAsRead(recipientId);
            }
            else if (_chatConnectionManager.GetUserConnections(recipientId).Any())
            {
                message.MarkAsDelivered();
            }
            else
            {
                message.MarkAsSent();
            }

            conversation.AddMessage(message);

            await _unitOfWork.MessageRepository.AddAsync(message);
            _unitOfWork.ConversationRepository.Update(conversation);
            await _unitOfWork.SaveChangesAsync();

            bool wasReadImmediately = message.Status == ClinicHub.Domain.Enums.MessageStatus.Read;

            // Real-time notifications
            var messageDto = _mapper.Map<MessageDto>(message);
            
            // Populate nested ReplyToMessageDto if this was a reply
            if (repliedMessage != null)
            {
                var repliedSender = await _unitOfWork.GetRepository<ApplicationUser, Guid>().FindByKeyAsync(repliedMessage.SenderId);
                messageDto.ReplyToMessage = new ReplyToMessageDto
                {
                    Id = repliedMessage.Id,
                    SenderId = repliedMessage.SenderId,
                    SenderName = repliedSender?.FullName ?? "Unknown",
                    Content = repliedMessage.Content,
                    CreatedAt = repliedMessage.CreatedAt
                };
            }
            
            // Fetch sender info for the DTO
            var sender = await _unitOfWork.GetRepository<ApplicationUser, Guid>().FindByKeyAsync(currentUserId);
            messageDto.SenderName = sender?.FullName ?? "Unknown";
            messageDto.SenderProfilePictureUrl = sender?.ProfilePictureUrl ?? string.Empty;
            
            // 1. Notify the recipient about the new message (for their active chat window)
            await _pusherService.TriggerEventAsync($"private-user-{recipientId}", "new-message", messageDto);

            // 2. Notify both participants that the conversation has been updated (for their sidebar list)
            var updateData = new { 
                conversationId = conversation.Id, 
                lastMessage = messageDto.Content, 
                lastMessageDate = messageDto.CreatedAt 
            };
            await _pusherService.TriggerEventAsync($"private-user-{currentUserId}", "conversation-updated", updateData);
            await _pusherService.TriggerEventAsync($"private-user-{recipientId}", "conversation-updated", updateData);

            // 3. If read immediately, notify the sender to update ticks
            if (wasReadImmediately)
            {
                await _pusherService.TriggerEventAsync(
                    $"private-user-{currentUserId}", 
                    "messages-read", 
                    new { conversationId = request.ConversationId }
                );
            }

            return messageDto;
        }
    }
}
