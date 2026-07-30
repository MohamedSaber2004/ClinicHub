using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.ActivateClinic
{
    public class ActivateClinicCommandHandler : IRequestHandler<ActivateClinicCommand, ClinicManagementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivateClinicCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IStringLocalizer<Messages> localizer,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizer = localizer;
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<ClinicManagementDto> Handle(ActivateClinicCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository
                .GetAllAsync(null)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (clinic == null)
            {
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);
            }

            if (clinic.ClinicAdminId.HasValue)
            {
                var adminUser = await _userManager.FindByIdAsync(clinic.ClinicAdminId.Value.ToString());
                if (adminUser != null)
                {
                    adminUser.IsDeleted = false;
                    adminUser.IsActive = true;
                    adminUser.DeletedAt = null;
                    await _userManager.UpdateAsync(adminUser);
                }
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
