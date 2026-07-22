using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Specializations.Commands.DeleteSpecialization
{
    public class DeleteSpecializationCommandHandler : IRequestHandler<DeleteSpecializationCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly ICurrentUserService _currentUserService;

        public DeleteSpecializationCommandHandler(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(DeleteSpecializationCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.SpecializationRepository;
            var specialization = await repo.GetByIdAsync(request.Id);

            if (specialization == null)
            {
                return JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.SpecializationMessages.NotFound.Value]);
            }

            specialization.MarkAsDeleted(_currentUserService.UserId.ToString());
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Success.Value]):
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Error.Value]);
        }
    }
}
