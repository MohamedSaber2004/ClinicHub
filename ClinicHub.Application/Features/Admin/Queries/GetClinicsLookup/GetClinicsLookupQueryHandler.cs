using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetClinicsLookup
{
    public class GetClinicsLookupQueryHandler : IRequestHandler<GetClinicsLookupQuery, List<ClinicLookupDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetClinicsLookupQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ClinicLookupDto>> Handle(GetClinicsLookupQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ClinicRepository
                .GetAllAsync(null)
                .ProjectTo<ClinicLookupDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
