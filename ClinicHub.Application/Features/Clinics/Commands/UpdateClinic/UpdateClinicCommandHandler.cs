using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.UpdateClinic
{
    public class UpdateClinicCommandHandler : IRequestHandler<UpdateClinicCommand, ClinicManagementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly ICurrentUserService _currentUserService;

        public UpdateClinicCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IStringLocalizer<Messages> localizer,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizer = localizer;
            _currentUserService = currentUserService;
        }

        public async Task<ClinicManagementDto> Handle(UpdateClinicCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.Id);
            if (clinic == null)
            {
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);
            }

            var dto = request.Dto;
            var updatedBy = _currentUserService.IsAuthenticated
                ? _currentUserService.UserId.ToString()
                : "system";

            clinic.UpdateDetails(
                dto.Name,
                dto.NameAr,
                dto.Description,
                dto.ArDescription,
                dto.Address,
                dto.AddressAr,
                dto.Phone,
                dto.Email,
                dto.Website,
                dto.Logo,
                dto.WorkingHours,
                dto.SpecializationId,
                updatedBy,
                dto.WorkingHoursStart,
                dto.WorkingHoursEnd,
                dto.WorkingDays != null ? string.Join(",", dto.WorkingDays) : null);

            _unitOfWork.ClinicRepository.Update(clinic);
            await _unitOfWork.SaveChangesAsync();

            var clinicDto = _mapper.Map<ClinicManagementDto>(clinic);
            return clinicDto;
        }
    }
}
