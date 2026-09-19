using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Availability.Commands.DeleteAvailability
{
    public class DeleteAvailabilityCommandHandler : IRequestHandler<DeleteAvailabilityCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteAvailabilityCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<string> Handle(DeleteAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.DoctorAvailabilityRepository;
            var availability = await repo.GetByIdAsync(request.Id);

            var scopeClinicId = await ClinicScopeResolver.ResolveClinicIdAsync(
                _unitOfWork, _currentUserService.UserId, _currentUserService.CurrentClinicId, cancellationToken);
            if (!scopeClinicId.HasValue || availability.ClinicId != scopeClinicId.Value)
                throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            availability.MarkAsDeleted(_currentUserService.UserId.ToString());
            repo.Update(availability);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ?
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ValidationMessages.DeletedSuccessfully.Value]) :
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ValidationMessages.DeletedFailed.Value]);
        }
    }
}
