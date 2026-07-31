using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.CreateMyAvailability
{
    public class CreateMyAvailabilityCommandHandler : IRequestHandler<CreateMyAvailabilityCommand, AvailabilityDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CreateMyAvailabilityCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<AvailabilityDto> Handle(CreateMyAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            if (doctor.ClinicId == null)
                throw new BadRequestException(LocalizationKeys.AvailabilityMessages.DoctorNotAssignedToClinic.Value);

            var availability = new DoctorAvailability(
                doctor.Id,
                doctor.ClinicId.Value,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.SlotDurationMinutes);

            availability.MarkAsCreated(_currentUserService.UserId.ToString());

            await _unitOfWork.DoctorAvailabilityRepository.AddAsync(availability);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AvailabilityDto>(availability);
        }
    }
}
