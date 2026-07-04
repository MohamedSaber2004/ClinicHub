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

namespace ClinicHub.Application.Features.Admin.Queries.GetClinicAuditLogs
{
    public class GetClinicAuditLogsQueryHandler : IRequestHandler<GetClinicAuditLogsQuery, PagginatedResult<AuditLogDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public GetClinicAuditLogsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<PagginatedResult<AuditLogDto>> Handle(GetClinicAuditLogsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserTypes is null || (_currentUser.UserTypes.Value & (int)UserType.SuperAdmin) == 0)
                throw new UnAuthorizedException();

            var repo = _unitOfWork.GetRepository<AuditLog, Guid>();
            var query = repo.GetAllAsync(l => l.ClinicId == request.ClinicId);

            if (request.FromDate.HasValue)
                query = query.Where(l => l.Timestamp >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(l => l.Timestamp <= request.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(request.Action))
                query = query.Where(l => l.Action == request.Action);

            query = query.OrderByDescending(l => l.Timestamp);

            var items = await query
                .ProjectTo<AuditLogDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);

            return items;
        }
    }
}
