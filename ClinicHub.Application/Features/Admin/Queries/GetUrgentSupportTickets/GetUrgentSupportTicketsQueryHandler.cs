using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetUrgentSupportTickets
{
    public class GetUrgentSupportTicketsQueryHandler : IRequestHandler<GetUrgentSupportTicketsQuery, List<SupportTicketDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUrgentSupportTicketsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<SupportTicketDto>> Handle(GetUrgentSupportTicketsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.GetRepository<SupportTicket, Guid>()
                .GetAllWithIncluding(
                    t => t.Priority == SupportTicketPriority.Urgent
                        && (t.Status == SupportTicketStatus.Open || t.Status == SupportTicketStatus.InProgress),
                    t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ProjectTo<SupportTicketDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
