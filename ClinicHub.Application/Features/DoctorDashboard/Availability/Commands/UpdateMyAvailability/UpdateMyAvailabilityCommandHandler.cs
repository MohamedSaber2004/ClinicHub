using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.UpdateMyAvailability
{
    public class UpdateMyAvailabilityCommandHandler : IRequestHandler<UpdateMyAvailabilityCommand, AvailabilityDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdateMyAvailabilityCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<AvailabilityDto> Handle(UpdateMyAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            var availability = await _unitOfWork.DoctorAvailabilityRepository.GetByIdAsync(request.Id);
            if (availability == null)
                throw new NotFoundException(LocalizationKeys.AvailabilityMessages.NotFound.Value);

            if (availability.DoctorId != doctor.Id)
                throw new ForbiddenException(LocalizationKeys.AvailabilityMessages.NotOwnedByDoctor.Value);

            availability.Update(
                request.DayOfWeek ?? availability.DayOfWeek,
                request.StartTime ?? availability.StartTime,
                request.EndTime ?? availability.EndTime,
                request.SlotDurationMinutes ?? availability.SlotDurationMinutes);

            availability.MarkAsUpdated(_currentUserService.UserId.ToString());

            _unitOfWork.DoctorAvailabilityRepository.Update(availability);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AvailabilityDto>(availability);
        }
    }
}
