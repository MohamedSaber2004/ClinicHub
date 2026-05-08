using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Queries.GetConversationById
{
    public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IPusherService _pusherService;

        public GetConversationByIdQueryHandler(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUserService, 
            IStringLocalizer<Messages> localizer,
            IPusherService pusherService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _pusherService = pusherService;
        }

        public async Task<ConversationDetailDto> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            var conversation = await _unitOfWork.ConversationRepository.GetByIdWithMessagesAsync(request.ConversationId, cancellationToken);
            if (conversation == null)
                throw new NotFoundException(_localizer[LocalizationKeys.ValidationMessages.ConversationNotFound.Key]);

            if (conversation.InitiatorId != currentUserId && conversation.RecipientId != currentUserId)
                throw new BadRequestException(_localizer[LocalizationKeys.ValidationMessages.UnauthorizedAction.Key]);

            // Fetch participants data
            var initiator = await _unitOfWork.GetRepository<ApplicationUser, Guid>().FindByKeyAsync(conversation.InitiatorId);
            var recipient = await _unitOfWork.GetRepository<ApplicationUser, Guid>().FindByKeyAsync(conversation.RecipientId);

            // Mark unread messages from the other user as read
            var unreadMessages = conversation.Messages
                .Where(m => m.SenderId != currentUserId && m.Status != ClinicHub.Domain.Enums.MessageStatus.Read)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.MarkAsRead(currentUserId);
                }
                await _unitOfWork.SaveChangesAsync();

                // Notify the sender that their messages have been read
                var senderId = unreadMessages.First().SenderId;
                await _pusherService.TriggerEventAsync(
                    $"private-user-{senderId}", 
                    "messages-read", 
                    new { conversationId = request.ConversationId }
                );
            }

            // Map messages with sender information
            var messageDtos = conversation.Messages
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.SenderId == conversation.InitiatorId 
                        ? initiator?.FullName ?? "Unknown" 
                        : recipient?.FullName ?? "Unknown",
                    SenderProfilePictureUrl = m.SenderId == conversation.InitiatorId 
                        ? initiator?.ProfilePictureUrl ?? string.Empty 
                        : recipient?.ProfilePictureUrl ?? string.Empty,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt,
                    ConversationId = m.ConversationId
                })
                .ToList();

            return new ConversationDetailDto
            {
                Id = conversation.Id,
                InitiatorId = conversation.InitiatorId,
                InitiatorName = initiator?.FullName ?? "Unknown",
                InitiatorProfilePictureUrl = initiator?.ProfilePictureUrl ?? string.Empty,
                RecipientId = conversation.RecipientId,
                RecipientName = recipient?.FullName ?? "Unknown",
                RecipientProfilePictureUrl = recipient?.ProfilePictureUrl ?? string.Empty,
                LastMessageDate = conversation.LastMessageDate,
                LastMessageContent = conversation.LastMessageContent,
                CreatedAt = conversation.CreatedAt,
                Messages = messageDtos
            };
        }
    }
}
