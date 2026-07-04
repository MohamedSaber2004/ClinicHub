using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetUrgentSupportTickets
{
    public class GetUrgentSupportTicketsQueryHandler : IRequestHandler<GetUrgentSupportTicketsQuery, PagginatedResult<SupportTicketDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public GetUrgentSupportTicketsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<PagginatedResult<SupportTicketDto>> Handle(GetUrgentSupportTicketsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserTypes is null || (_currentUser.UserTypes.Value & (int)UserType.SuperAdmin) == 0)
                throw new UnAuthorizedException();

            var query = _unitOfWork.GetRepository<SupportTicket, Guid>()
                .GetAllAsync(t => t.Priority == SupportTicketPriority.Urgent && t.Status != SupportTicketStatus.Closed)
                .OrderByDescending(t => t.CreatedAt);

            var items = await query
                .ProjectTo<SupportTicketDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);

            return items;
        }
    }
}
