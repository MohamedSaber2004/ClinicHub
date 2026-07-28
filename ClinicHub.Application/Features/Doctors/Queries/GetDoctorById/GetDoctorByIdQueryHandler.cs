using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Doctors.Queries.GetDoctorById
{
    public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, DoctorDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetDoctorByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DoctorDto> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.Id == request.Id)
                .Include(d => d.User)
                .Include(d => d.Clinic)
                .Include(d => d.Specialization)
                .Include(d => d.Availabilities)
                .FirstOrDefaultAsync(cancellationToken);

            if (doctor == null)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            var dto = _mapper.Map<DoctorDto>(doctor);
            dto.Availabilities = doctor.Availabilities
                .Where(a => !a.IsDeleted)
                .Select(_mapper.Map<DoctorAvailabilityDto>)
                .ToList();

            return dto;
        }
    }
}
