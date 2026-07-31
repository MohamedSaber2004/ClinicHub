using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.DeleteMyAvailability
{
    public class DeleteMyAvailabilityCommandHandler : IRequestHandler<DeleteMyAvailabilityCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteMyAvailabilityCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<string> Handle(DeleteMyAvailabilityCommand request, CancellationToken cancellationToken)
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

            availability.MarkAsDeleted(_currentUserService.UserId.ToString());

            _unitOfWork.DoctorAvailabilityRepository.Update(availability);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0
                ? _localizer[LocalizationKeys.AvailabilityMessages.Deleted.Value]
                : _localizer[LocalizationKeys.ValidationMessages.DeletedFailed.Value];
        }
    }
}
