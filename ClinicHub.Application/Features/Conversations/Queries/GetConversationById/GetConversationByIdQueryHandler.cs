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

        public GetConversationByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
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
            var initiator = await _unitOfWork.GetRepository<ApplicationUser, Guid>().GetByIdAsync(conversation.InitiatorId);
            var recipient = await _unitOfWork.GetRepository<ApplicationUser, Guid>().GetByIdAsync(conversation.RecipientId);

            // Map messages with sender information
            var messageDtos = conversation.Messages
                .Where(m => !m.IsDeleted)
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
                    CreatedAt = m.CreatedAt
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
                CreatedAt = conversation.CreatedAt,
                Messages = messageDtos
            };
        }
    }
}
