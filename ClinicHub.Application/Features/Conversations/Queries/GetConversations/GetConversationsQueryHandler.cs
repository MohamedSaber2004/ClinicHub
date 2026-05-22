using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Conversations.Queries.GetConversations
{
    public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, PagginatedResult<ConversationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetConversationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PagginatedResult<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            var conversations = await _unitOfWork.ConversationRepository.GetConversationsByUserIdAsync(currentUserId, cancellationToken);
            var totalCount = conversations.Count();

            var paginatedConversations = conversations
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var conversationDtos = new List<ConversationDto>();
            var userRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();

            foreach (var conversation in paginatedConversations)
            {
                var initiator = await userRepo.FindByKeyAsync(conversation.InitiatorId);
                var recipient = await userRepo.FindByKeyAsync(conversation.RecipientId);

                var lastMessage = conversation.Messages
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                var (contentType, mediaType, mediaFileName) = ExtractLastMessageContent(lastMessage);

                conversationDtos.Add(new ConversationDto
                {
                    Id = conversation.Id,
                    Name = conversation.Name,
                    GroupPhotoUrl = conversation.GroupPhotoUrl,
                    IsGroup = conversation.IsGroup,
                    InitiatorId = conversation.InitiatorId,
                    InitiatorName = initiator?.FullName ?? "Unknown",
                    InitiatorProfilePictureUrl = initiator?.ProfilePictureUrl ?? string.Empty,
                    RecipientId = conversation.RecipientId,
                    RecipientName = recipient?.FullName ?? "Unknown",
                    RecipientProfilePictureUrl = recipient?.ProfilePictureUrl ?? string.Empty,
                    LastMessageDate = lastMessage?.CreatedAt,
                    LastMessageContent = lastMessage?.Content,
                    LastMessageContentType = contentType,
                    LastMessageMediaType = mediaType,
                    LastMessageMediaFileName = mediaFileName,
                    CreatedAt = conversation.CreatedAt,
                    UnreadMessageCount = conversation.Messages.Count(m => m.SenderId != currentUserId && m.Status != ClinicHub.Domain.Enums.MessageStatus.Read)
                });
            }

            return new PagginatedResult<ConversationDto>(
                conversationDtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }

        private (LastMessageContentType contentType, MediaType? mediaType, string? mediaFileName) ExtractLastMessageContent(Message? lastMessage)
        {
            if (lastMessage == null)
                return (DTOs.LastMessageContentType.Text, null, null);

            var hasContent = !string.IsNullOrWhiteSpace(lastMessage.Content);
            var hasMedia = lastMessage.Media?.Any() ?? false;

            if (!hasContent && !hasMedia)
                return (LastMessageContentType.Text, null, null);

            LastMessageContentType contentType;
            if (hasContent && hasMedia)
                contentType = LastMessageContentType.TextAndMedia;
            else if (hasMedia)
                contentType = LastMessageContentType.Media;
            else
                contentType = LastMessageContentType.Text;

            MediaType? mediaType = null;
            string? mediaFileName = null;

            if (hasMedia && lastMessage.Media != null)
            {
                var firstMedia = lastMessage.Media.FirstOrDefault();
                if (firstMedia != null)
                {
                    mediaType = firstMedia.MediaType;
                    mediaFileName = firstMedia.FileName;
                }
            }

            return (contentType, mediaType, mediaFileName);
        }
    }
}
