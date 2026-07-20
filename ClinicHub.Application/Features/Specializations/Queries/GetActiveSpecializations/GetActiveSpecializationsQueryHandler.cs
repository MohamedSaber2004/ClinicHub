using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ClinicHub.Application.Features.Specializations.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Specializations.Queries.GetActiveSpecializations
{
    public class GetActiveSpecializationsQueryHandler : IRequestHandler<GetActiveSpecializationsQuery, List<SpecializationLookupDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetActiveSpecializationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<SpecializationLookupDto>> Handle(GetActiveSpecializationsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.SpecializationRepository
                .GetAllAsync(null)
                .Where(s => s.IsActive && !s.IsDeleted)
                .ProjectTo<SpecializationLookupDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
