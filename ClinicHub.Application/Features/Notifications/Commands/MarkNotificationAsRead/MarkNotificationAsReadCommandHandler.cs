using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicHub.Application.Features.Notifications.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFcmService _fcmService;

        public MarkNotificationAsReadCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fcmService = fcmService;
        }

        public async Task<int> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var notification = await _unitOfWork.NotificationRepository
                .GetFirstAsync(n => n.Id == request.Id && n.UserId == userId, cancellationToken);

            if (notification is not null && !notification.IsRead)
            {
                notification.MarkAsRead();
                await _unitOfWork.SaveChangesAsync();
            }

            var remainingUnread = await _unitOfWork.NotificationRepository
                .GetAllAsync(n => n.UserId == userId && n.IsRead == false)
                .CountAsync(cancellationToken);

            await _fcmService.SendBadgeUpdateAsync(userId, remainingUnread);

            return remainingUnread;
        }
    }
}
