using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Availability.Commands.DeleteAvailability
{
    public class DeleteAvailabilityCommandHandler : IRequestHandler<DeleteAvailabilityCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteAvailabilityCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(DeleteAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.DoctorAvailabilityRepository;
            var availability = await repo.GetByIdAsync(request.Id);

            availability.MarkAsDeleted(_currentUserService.UserId.ToString());
            repo.Update(availability);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ?
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.DeletedSuccessfully.Value) :
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.DeletedFailed.Value);
        }
    }
}
