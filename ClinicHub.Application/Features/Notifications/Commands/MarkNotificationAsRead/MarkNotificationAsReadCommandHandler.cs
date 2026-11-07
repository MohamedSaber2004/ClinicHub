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

        public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            // The paginated list deletes the rows it returns, so a missing row already counts as read.
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

            return remainingUnread;
        }
    }
}
