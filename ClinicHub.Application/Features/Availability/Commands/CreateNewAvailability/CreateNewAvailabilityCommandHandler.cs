using AutoMapper;
using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Availability.Commands.CreateNewAvailability
{
    public class CreateNewAvailabilityCommandHandler : IRequestHandler<CreateNewAvailabilityCommand, AvailabilityDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public CreateNewAvailabilityCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<AvailabilityDto> Handle(CreateNewAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetByIdAsync(request.DoctorId);

            if (doctor.ClinicId == null)
                throw new BadRequestException("Doctor must be assigned to a clinic to create availability");

            var scopeClinicId = await ClinicScopeResolver.ResolveClinicIdAsync(
                _unitOfWork, _currentUser.UserId, _currentUser.CurrentClinicId, cancellationToken);
            if (!scopeClinicId.HasValue || doctor.ClinicId.Value != scopeClinicId.Value)
                throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

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
