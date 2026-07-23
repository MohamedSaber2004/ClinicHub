using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetAllSupportTickets
{
    public class GetAllSupportTicketsQueryHandler : IRequestHandler<GetAllSupportTicketsQuery, PagginatedResult<SupportTicketDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllSupportTicketsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<SupportTicketDto>> Handle(GetAllSupportTicketsQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.GetRepository<SupportTicket, Guid>()
                .GetAllWithIncluding(t => true, t => t.User)
                .AsQueryable();

            if (request.Status.HasValue)
                query = query.Where(t => t.Status == request.Status.Value);

            if (request.Priority.HasValue)
                query = query.Where(t => t.Priority == request.Priority.Value);

            if (request.ClinicId.HasValue)
                query = query.Where(t => t.ClinicId == request.ClinicId.Value);

            query = query.OrderByDescending(t => t.CreatedAt);

            return await query
                .ProjectTo<SupportTicketDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
