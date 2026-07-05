using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicHub.Application.Features.Notifications.Queries.GetNotificationsCount
{
    public class GetNotificationsCountQueryHandler : IRequestHandler<GetNotificationsCountQuery, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetNotificationsCountQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(GetNotificationsCountQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var query = _unitOfWork.NotificationRepository
                .GetAllAsync(n => n.UserId == userId);

            if (request.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == request.IsRead.Value);
            }

            var count = await query.CountAsync(cancellationToken);
            return count;
        }
    }
}
