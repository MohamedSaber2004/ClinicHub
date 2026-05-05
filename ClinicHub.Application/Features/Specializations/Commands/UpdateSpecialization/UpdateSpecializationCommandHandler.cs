using AutoMapper;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Specializations.Commands.UpdateSpecialization
{
    public class UpdateSpecializationCommandHandler : IRequestHandler<UpdateSpecializationCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateSpecializationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<string> Handle(UpdateSpecializationCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.SpecializationRepository;
            var specialization = await repo.GetByIdAsync(request.Id);

            if (specialization == null)
            {
                return JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.SpecializationMessages.NotFound.Value);
            }

            _mapper.Map(request, specialization);
            repo.Update(specialization);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ? 
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Success.Value):
                JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.GeneralMessages.Error.Value);
        }
    }
}
