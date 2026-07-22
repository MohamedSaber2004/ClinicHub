using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Specializations.Commands.UpdateSpecialization
{
    public class UpdateSpecializationCommandHandler : IRequestHandler<UpdateSpecializationCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IImageValidator _imageValidator;
        private const int SpecializationIconsPlace = 13;

        public UpdateSpecializationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<Messages> localizer, IImageValidator imageValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizer = localizer;
            _imageValidator = imageValidator;
        }

        public async Task<string> Handle(UpdateSpecializationCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.SpecializationRepository;
            var specialization = await repo.GetAllAsync(null).IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == request.Id);

            if (specialization == null)
            {
                return JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.SpecializationMessages.NotFound.Value);
            }

            _mapper.Map(request, specialization);

            if (request.IsActive)
                specialization.Active();
            else
                specialization.Deactive();

            repo.Update(specialization);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return saveResult > 0 ?
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Success.Value]) :
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Error.Value]);
        }
    }
}
