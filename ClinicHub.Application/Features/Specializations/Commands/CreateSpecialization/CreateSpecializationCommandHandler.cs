using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Specializations.Commands.CreateSpecialization
{
    public class CreateSpecializationCommandHandler : IRequestHandler<CreateSpecializationCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IImageValidator _imageValidator;

        public CreateSpecializationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<Messages> localizer, IImageValidator imageValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizer = localizer;
            _imageValidator = imageValidator;
        }

        public async Task<string> Handle(CreateSpecializationCommand request, CancellationToken cancellationToken)
        {
            var specialization = _mapper.Map<Specialization>(request);

            if (request.Icon is not null)
            {
                var (uploaded, result) = await _imageValidator.UploadImage(request.Icon, 13);
                if (!uploaded)
                    return result;

                specialization.IconUrl = result;
            }

            var repo = _unitOfWork.SpecializationRepository;
            await repo.AddAsync(specialization);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return saveResult > 0 ?
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Success.Value]) :
                JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.GeneralMessages.Error.Value]);
        }
    }
}
