using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Clinics.Queries.GetPaginatedClinics
{
    public class GetPaginatedClinicsQueryHandler : IRequestHandler<GetPaginatedClinicsQuery, PagginatedResult<ClinicManagementDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPaginatedClinicsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<ClinicManagementDto>> Handle(GetPaginatedClinicsQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.ClinicRepository.GetAllAsync(null);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    (c.NameAr != null && c.NameAr.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)) ||
                    (c.Phone != null && c.Phone.Contains(term)) ||
                    (c.Description != null && c.Description.ToLower().Contains(term)));
            }

            if (request.Status.HasValue)
                query = query.Where(c => c.Status == request.Status.Value);

            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(c => c.Name.ToLower().Contains(request.Name.ToLower()));

            if (!string.IsNullOrWhiteSpace(request.Email))
                query = query.Where(c => c.Email != null && c.Email.ToLower().Contains(request.Email.ToLower()));

            if (!string.IsNullOrWhiteSpace(request.Phone))
                query = query.Where(c => c.Phone != null && c.Phone.Contains(request.Phone));

            if (request.CreatedFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= request.CreatedFrom.Value);

            if (request.CreatedTo.HasValue)
                query = query.Where(c => c.CreatedAt <= request.CreatedTo.Value);

            query = (request.SortBy?.ToLower()) switch
            {
                "name" => request.SortAscending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "status" => request.SortAscending ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status),
                _ => request.SortAscending ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt)
            };

            if (request.PageNumber == -1)
            {
                var allItems = await query
                    .ProjectTo<ClinicManagementDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return new PagginatedResult<ClinicManagementDto>(allItems, allItems.Count, 1, allItems.Count);
            }

            return await query
                .ProjectTo<ClinicManagementDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
