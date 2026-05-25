using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Commands.AuthenticatePusher
{
    public class AuthenticatePusherCommandHandler : IRequestHandler<AuthenticatePusherCommand, string>
    {
        private readonly IPusherService _pusherService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IChatConnectionManager _chatConnectionManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<Messages> _localizer;

        public AuthenticatePusherCommandHandler(
            IPusherService pusherService,
            ICurrentUserService currentUserService,
            IChatConnectionManager chatConnectionManager,
            IUnitOfWork unitOfWork,
            IMediator mediator,
            IStringLocalizer<Messages> localizer)
        {
            _pusherService = pusherService;
            _currentUserService = currentUserService;
            _chatConnectionManager = chatConnectionManager;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _localizer = localizer;
        }

        public async Task<string> Handle(AuthenticatePusherCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.SocketId) || string.IsNullOrEmpty(request.ChannelName))
                throw new BadRequestException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.RealTimeMessages.MissingSocketInfo.Value]));

            var userId = _currentUserService.UserId;
            var user = await _unitOfWork.GetRepository<ApplicationUser, Guid>().GetByIdAsync(userId);

            var userInfo = new
            {
                id = userId.ToString(),
                name = user?.FullName ?? "Unknown",
                avatar = user?.ProfilePictureUrl ?? ""
            };

            var authJson = _pusherService.Authenticate(request.ChannelName, request.SocketId, userId.ToString(), userInfo);

            _chatConnectionManager.ConnectUser(userId, request.SocketId);

            // Bulk deliver messages received while offline
            await _mediator.Send(new DeliverPendingMessages.DeliverPendingMessagesCommand(), cancellationToken);

            return authJson;
        }
    }
}
