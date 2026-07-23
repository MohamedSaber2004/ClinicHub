using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetClinicAuditLogs
{
    public class GetClinicAuditLogsQueryHandler : IRequestHandler<GetClinicAuditLogsQuery, PagginatedResult<AuditLogDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetClinicAuditLogsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<AuditLogDto>> Handle(GetClinicAuditLogsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.GetRepository<AuditLog, Guid>()
                .GetAllWithIncluding(l => l.ClinicId == request.ClinicId, l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .ProjectTo<AuditLogDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
