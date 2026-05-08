using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Queries.GetConversationMessages
{
    public class GetConversationMessagesQueryHandler : IRequestHandler<GetConversationMessagesQuery, PagginatedResult<MessageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetConversationMessagesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<PagginatedResult<MessageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            var conversation = await _unitOfWork.ConversationRepository.GetByIdAsync(request.ConversationId);
            if (conversation == null)
                throw new NotFoundException(_localizer[LocalizationKeys.ValidationMessages.ConversationNotFound.Key]);

            if (conversation.InitiatorId != currentUserId && conversation.RecipientId != currentUserId)
                throw new BadRequestException(_localizer[LocalizationKeys.ValidationMessages.UnauthorizedAction.Key]);

            var messages = await _unitOfWork.MessageRepository.GetMessagesByConversationIdAsync(request.ConversationId, cancellationToken);
            var totalCount = messages.Count();

            var paginatedMessages = messages
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var messageDtos = new List<MessageDto>();
            var userRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();
            var initiator = await userRepo.FindByKeyAsync(conversation.InitiatorId);
            var recipient = await userRepo.FindByKeyAsync(conversation.RecipientId);

            foreach (var message in paginatedMessages)
            {
                var sender = message.SenderId == initiator?.Id ? initiator : recipient;
                messageDtos.Add(new MessageDto
                {
                    Id = message.Id,
                    SenderId = message.SenderId,
                    SenderName = sender?.FullName ?? "Unknown",
                    SenderProfilePictureUrl = sender?.ProfilePictureUrl ?? string.Empty,
                    Content = message.Content,
                    IsRead = message.IsRead,
                    CreatedAt = message.CreatedAt,
                    ConversationId = message.ConversationId
                });
            }

            return new PagginatedResult<MessageDto>(
                messageDtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
