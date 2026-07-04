using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Doctors.Queries.GetDoctorsByClinic
{
    public class GetDoctorsByClinicQueryHandler : IRequestHandler<GetDoctorsByClinicQuery, List<DoctorDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetDoctorsByClinicQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<DoctorDto>> Handle(GetDoctorsByClinicQuery request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Id == request.ClinicId, cancellationToken);
            if (!clinic)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var doctors = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.ClinicId == request.ClinicId)
                .Include(d => d.User)
                .Include(d => d.Clinic)
                .Include(d => d.Specialization)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<DoctorDto>>(doctors);
        }
    }
}
