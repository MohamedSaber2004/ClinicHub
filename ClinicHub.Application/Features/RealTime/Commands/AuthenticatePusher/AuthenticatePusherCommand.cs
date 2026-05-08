using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.AuthenticatePusher
{
    public class AuthenticatePusherCommand : IRequest<string>
    {
        public string SocketId { get; set; } = null!;
        public string ChannelName { get; set; } = null!;
    }

    public class AuthenticatePusherCommandHandler : IRequestHandler<AuthenticatePusherCommand, string>
    {
        private readonly IPusherService _pusherService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IChatConnectionManager _chatConnectionManager;
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticatePusherCommandHandler(
            IPusherService pusherService,
            ICurrentUserService currentUserService,
            IChatConnectionManager chatConnectionManager,
            IUnitOfWork unitOfWork)
        {
            _pusherService = pusherService;
            _currentUserService = currentUserService;
            _chatConnectionManager = chatConnectionManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Handle(AuthenticatePusherCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.SocketId) || string.IsNullOrEmpty(request.ChannelName))
                throw new BadRequestException(LocalizationKeys.RealTimeMessages.MissingSocketInfo);

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

            return authJson;
        }
    }
}
