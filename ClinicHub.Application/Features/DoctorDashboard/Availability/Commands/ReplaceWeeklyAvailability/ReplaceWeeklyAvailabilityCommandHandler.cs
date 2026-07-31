using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.ReplaceWeeklyAvailability
{
    public class ReplaceWeeklyAvailabilityCommandHandler : IRequestHandler<ReplaceWeeklyAvailabilityCommand, List<AvailabilityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public ReplaceWeeklyAvailabilityCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<List<AvailabilityDto>> Handle(ReplaceWeeklyAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            if (doctor.ClinicId == null)
                throw new BadRequestException(LocalizationKeys.AvailabilityMessages.DoctorNotAssignedToClinic.Value);

            var createdBy = _currentUserService.UserId.ToString();

            var existing = await _unitOfWork.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == doctor.Id && !a.IsDeleted)
                .ToListAsync(cancellationToken);

            var incomingIds = request.Days
                .Where(d => d.Id.HasValue && d.Id != Guid.Empty)
                .Select(d => d.Id!.Value)
                .ToHashSet();

            foreach (var input in request.Days)
            {
                if (input.Id.HasValue && input.Id != Guid.Empty)
                {
                    var availability = existing.FirstOrDefault(a => a.Id == input.Id);
                    if (availability == null)
                        throw new NotFoundException(LocalizationKeys.AvailabilityMessages.NotFound.Value);

                    availability.Update(input.DayOfWeek, input.StartTime, input.EndTime, input.SlotDurationMinutes);
                    availability.MarkAsUpdated(createdBy);
                    _unitOfWork.DoctorAvailabilityRepository.Update(availability);
                }
                else
                {
                    var availability = new DoctorAvailability(
                        doctor.Id,
                        doctor.ClinicId.Value,
                        input.DayOfWeek,
                        input.StartTime,
                        input.EndTime,
                        input.SlotDurationMinutes);
                    availability.MarkAsCreated(createdBy);
                    await _unitOfWork.DoctorAvailabilityRepository.AddAsync(availability);
                }
            }

            foreach (var availability in existing.Where(a => !incomingIds.Contains(a.Id)))
            {
                availability.MarkAsDeleted(createdBy);
                _unitOfWork.DoctorAvailabilityRepository.Update(availability);
            }

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == doctor.Id && !a.IsDeleted)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<AvailabilityDto>>(updated);
        }
    }
}
