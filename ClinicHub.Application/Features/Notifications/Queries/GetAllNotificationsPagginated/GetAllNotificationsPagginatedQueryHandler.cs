using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Notifications.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicHub.Application.Features.Notifications.Queries.GetAllNotificationsPagginated
{
    public class GetAllNotificationsPagginatedQueryHandler : IRequestHandler<GetAllNotificationsPagginatedQuery, PagginatedResult<NotificationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetAllNotificationsPagginatedQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PagginatedResult<NotificationDto>> Handle(GetAllNotificationsPagginatedQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var query = _unitOfWork.NotificationRepository
                .GetAllAsync(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var paginatedResult = await query
                .ProjectTo<NotificationDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);

            var ids = paginatedResult.Items.Select(n => n.Id).ToList();
            var notifications = await _unitOfWork.NotificationRepository
                .GetBy(n => ids.Contains(n.Id))
                .ToListAsync(cancellationToken);
            notifications.ForEach(n => n.MarkAsRead());
            await _unitOfWork.SaveChangesAsync();

            return paginatedResult;
        }
    }
}
