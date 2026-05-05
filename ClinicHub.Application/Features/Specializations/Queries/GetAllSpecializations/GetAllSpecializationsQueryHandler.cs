using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Specializations.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

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
                .ProjectTo<SpecializationDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize);

            return specializations;
        }
    }
}
