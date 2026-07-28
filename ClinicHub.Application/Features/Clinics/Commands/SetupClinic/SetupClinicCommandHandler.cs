using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;

namespace ClinicHub.Application.Features.Clinics.Commands.SetupClinic
{
    public sealed class SetupClinicCommandHandler : IRequestHandler<SetupClinicCommand, ClinicManagementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetupClinicCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<ClinicManagementDto> Handle(SetupClinicCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (user == null)
                throw new NotFoundException(_localizer[LocalizationKeys.ExceptionMessages.NotFound.Value]);

            var createdBy = _currentUserService.IsAuthenticated
                ? _currentUserService.UserId.ToString()
                : "system";

            var clinic = new Clinic
            {
                Name = dto.Name,
                NameAr = dto.Name,
                Description = dto.Description,
                ArDescription = dto.Description,
                Address = dto.Address,
                AddressAr = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                Website = dto.Website,
                Logo = dto.Logo,
                WorkingHours = dto.WorkingHours,
                WorkingHoursStart = dto.WorkingHoursStart,
                WorkingHoursEnd = dto.WorkingHoursEnd,
                WorkingDays = dto.WorkingDays != null ? string.Join(",", dto.WorkingDays) : null,
                SpecializationId = dto.SpecializationId,
                Location = new Point(dto.Lng, dto.Lat) { SRID = 4326 },
                IsRegistered = true,
                Status = ClinicStatus.Active,
                ClinicAdminId = user.Id
            };
            clinic.MarkAsCreated(createdBy);

            await _unitOfWork.ClinicRepository.AddAsync(clinic);

            var existingDoctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.UserId == user.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingDoctor == null)
            {
                var doctor = new Doctor(
                    user.Id,
                    clinic.Id,
                    dto.SpecializationId,
                    string.Empty,
                    0);
                doctor.MarkAsCreated(createdBy);
                await _unitOfWork.DoctorRepository.AddAsync(doctor);
            }
            else
            {
                existingDoctor.AssignToClinic(clinic.Id);
            }

            clinic.IsSetupComplete = true;

            user.AssignToClinic(clinic.Id);

            await _unitOfWork.SaveChangesAsync();

            var clinicDto = _mapper.Map<ClinicManagementDto>(clinic);
            return clinicDto;
        }
    }
}
