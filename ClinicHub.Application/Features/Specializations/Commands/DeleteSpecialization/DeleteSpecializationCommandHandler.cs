using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Specializations.Commands.DeleteSpecialization
{
    public class DeleteSpecializationCommandHandler : IRequestHandler<DeleteSpecializationCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSpecializationCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Handle(DeleteSpecializationCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.SpecializationRepository;
            var specialization = await repo.GetByIdAsync(request.Id);

            if (specialization == null)
            {
                return JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.SpecializationMessages.NotFound.Value);
            }

            repo.Delete(specialization);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value):
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
