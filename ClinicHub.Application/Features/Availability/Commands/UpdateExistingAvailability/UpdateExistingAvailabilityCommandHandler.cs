using AutoMapper;
using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Availability.Commands.UpdateExistingAvailability
{
    public class UpdateExistingAvailabilityCommandHandler : IRequestHandler<UpdateExistingAvailabilityCommand, AvailabilityDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public UpdateExistingAvailabilityCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<AvailabilityDto> Handle(UpdateExistingAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.DoctorAvailabilityRepository;
            var availability = await repo.GetByIdAsync(request.Id);

            var scopeClinicId = await ClinicScopeResolver.ResolveClinicIdAsync(
                _unitOfWork, _currentUser.UserId, _currentUser.CurrentClinicId, cancellationToken);
            if (!scopeClinicId.HasValue || availability.ClinicId != scopeClinicId.Value)
                throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            availability.Update(
                request.DayOfWeek ?? availability.DayOfWeek,
                request.StartTime ?? availability.StartTime,
                request.EndTime ?? availability.EndTime,
                request.SlotDurationMinutes ?? availability.SlotDurationMinutes);

            repo.Update(availability);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AvailabilityDto>(availability);
        }
    }
}
