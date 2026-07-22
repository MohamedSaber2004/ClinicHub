using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Specializations.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Specializations.Queries.GetAllSpecializations
{
    public class GetAllSpecializationsQueryHandler : IRequestHandler<GetAllSpecializationsQuery, PagginatedResult<SpecializationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllSpecializationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<SpecializationDto>> Handle(GetAllSpecializationsQuery request, CancellationToken cancellationToken)
        {
            var specializations = await _unitOfWork.SpecializationRepository
                .GetAllAsync(null)
                .IgnoreQueryFilters()
                .WhereIf(request.IsFamous.HasValue, s => s.IsFamous == request.IsFamous!.Value)
                .ProjectTo<SpecializationDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize);

            return specializations;
        }
    }
}
