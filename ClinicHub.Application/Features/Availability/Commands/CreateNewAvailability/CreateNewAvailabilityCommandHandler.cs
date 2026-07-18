using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Availability.Commands.CreateNewAvailability
{
    public class CreateNewAvailabilityCommandHandler : IRequestHandler<CreateNewAvailabilityCommand, AvailabilityDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateNewAvailabilityCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AvailabilityDto> Handle(CreateNewAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetByIdAsync(request.DoctorId);

            if (doctor.ClinicId == null)
                throw new BadRequestException("Doctor must be assigned to a clinic to create availability");

            var availability = new DoctorAvailability(
                request.DoctorId,
                doctor.ClinicId.Value,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.SlotDurationMinutes
            );

            await _unitOfWork.DoctorAvailabilityRepository.AddAsync(availability);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AvailabilityDto>(availability);
        }
    }
}
