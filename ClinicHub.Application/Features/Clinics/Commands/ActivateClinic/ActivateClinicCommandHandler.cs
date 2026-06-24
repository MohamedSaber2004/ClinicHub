using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.ActivateClinic
{
    public class ActivateClinicCommandHandler : IRequestHandler<ActivateClinicCommand, ClinicManagementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly ICurrentUserService _currentUserService;

        public ActivateClinicCommandHandler(
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

        public async Task<ClinicManagementDto> Handle(ActivateClinicCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.Id);
            if (clinic == null)
            {
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);
            }

            var updatedBy = _currentUserService.IsAuthenticated
                ? _currentUserService.UserId.ToString()
                : "system";

            clinic.UpdateStatus(ClinicStatus.Active, updatedBy);
            _unitOfWork.ClinicRepository.Update(clinic);
            await _unitOfWork.SaveChangesAsync();

            var clinicDto = _mapper.Map<ClinicManagementDto>(clinic);
            return clinicDto;
        }
    }
}
