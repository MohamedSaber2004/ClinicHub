using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetPendingClinics
{
    public class GetPendingClinicsQueryHandler : IRequestHandler<GetPendingClinicsQuery, List<ClinicManagementDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPendingClinicsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ClinicManagementDto>> Handle(GetPendingClinicsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ClinicRepository
                .GetAllAsync(c => c.Status == ClinicStatus.PendingApproval && !c.IsDeleted)
                .ProjectTo<ClinicManagementDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
